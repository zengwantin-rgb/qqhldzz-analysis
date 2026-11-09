#import "TraceControl.h"
#import "Scanner.h"
#import <math.h>
#import <TargetConditionals.h>

NSString *MJTracePreferenceKey(void) { return @"int_gmModelVMTraceBinLevel"; }
static BOOL MJTraceFail(NSError **error, NSString *message) {
    if (error) *error=[NSError errorWithDomain:@"MJTraceControl" code:1 userInfo:@{NSLocalizedDescriptionKey:message}];
    return NO;
}
static BOOL MJTraceInteger(id value) {
    if (![value isKindOfClass:NSNumber.class] || CFGetTypeID((__bridge CFTypeRef)value)==CFBooleanGetTypeID()) return NO;
    double n=[value doubleValue]; return isfinite(n) && n==[value longLongValue] && n>=-1 && n<=3;
}
static NSString *MJTracePath(NSString *container, NSError **error) {
    NSFileManager *fm=NSFileManager.defaultManager;
    NSString *absolute=container.stringByStandardizingPath;
    NSString *resolved=container.stringByResolvingSymlinksInPath;
    if ([resolved hasPrefix:@"/var/"]) resolved=[@"/private" stringByAppendingString:resolved];
    if ([resolved hasPrefix:@"/tmp/"]) resolved=[@"/private" stringByAppendingString:resolved];
    if ([absolute hasPrefix:@"/var/"]) absolute=[@"/private" stringByAppendingString:absolute];
    if ([absolute hasPrefix:@"/tmp/"]) absolute=[@"/private" stringByAppendingString:absolute];
    if (![absolute isEqual:resolved]) { MJTraceFail(error,@"目标路径含符号链接，未修改设置。"); return nil; }
    NSString *metaPath=[container stringByAppendingPathComponent:@".com.apple.mobile_container_manager.metadata.plist"];
    if ([[fm attributesOfItemAtPath:metaPath error:nil][NSFileType] isEqual:NSFileTypeSymbolicLink]) { MJTraceFail(error,@"容器元信息含符号链接，未修改。"); return nil; }
    NSDictionary *meta=[NSDictionary dictionaryWithContentsOfFile:metaPath];
    if (!MJMatchesBundle(meta[@"MCMMetadataIdentifier"])) { MJTraceFail(error,@"目标不是原包名欢乐麻将，未修改设置。"); return nil; }
    NSString *path=container;
    for (NSString *part in @[@"Library",@"Preferences",@"com.tencent.happymj.plist"]) {
        path=[path stringByAppendingPathComponent:part];
        NSDictionary *attr=[fm attributesOfItemAtPath:path error:error];
        if (!attr) return nil;
        if ([attr[NSFileType] isEqual:NSFileTypeSymbolicLink]) { MJTraceFail(error,@"设置路径含符号链接，未修改设置。"); return nil; }
    }
    NSDictionary *attr=[fm attributesOfItemAtPath:path error:error];
    if (![attr[NSFileType] isEqual:NSFileTypeRegular] || [attr[NSFileSize] unsignedLongLongValue]>4*1024*1024) { MJTraceFail(error,@"设置文件类型或大小不符合要求，未修改。"); return nil; }
    return path;
}
static NSMutableDictionary *MJTraceRead(NSString *path, NSData **snapshot, NSPropertyListFormat *format, NSError **error) {
    NSData *raw=[NSData dataWithContentsOfFile:path options:0 error:error];
    if (!raw) return nil;
    id plist=[NSPropertyListSerialization propertyListWithData:raw options:NSPropertyListMutableContainers format:format error:error];
    if (![plist isKindOfClass:NSMutableDictionary.class]) { MJTraceFail(error,@"设置文件无法读取为字典，未修改。"); return nil; }
    id existing=plist[MJTracePreferenceKey()];
    if (existing && !MJTraceInteger(existing)) { MJTraceFail(error,@"诊断设置不是预期整数，未修改。"); return nil; }
    if (snapshot) *snapshot=raw;
    return plist;
}
static NSString *MJTraceBackupPath(NSString *container, NSString *directory) {
    return [directory stringByAppendingPathComponent:[container.lastPathComponent stringByAppendingString:@"-trace.plist"]];
}
static BOOL MJTraceValidBackup(NSDictionary *backup, NSString *container) {
    return [backup[@"schema"] isEqual:@1] && [backup[@"container"] isEqual:container.lastPathComponent]
        && [backup[@"key"] isEqual:MJTracePreferenceKey()] && [backup[@"existed"] isKindOfClass:NSNumber.class]
        && (![backup[@"existed"] boolValue] || MJTraceInteger(backup[@"original"]));
}
NSDictionary *MJTraceStatus(NSString *container, NSString *backupDirectory, NSError **error) {
    NSString *path=MJTracePath(container,error); if (!path) return nil;
    NSMutableDictionary *plist=MJTraceRead(path,NULL,NULL,error); if (!plist) return nil;
    NSString *backupPath=MJTraceBackupPath(container,backupDirectory);
    BOOL backupExists=[NSFileManager.defaultManager fileExistsAtPath:backupPath];
    NSDictionary *backup=backupExists?[NSDictionary dictionaryWithContentsOfFile:backupPath]:nil;
    return @{@"preference_present":@(plist[MJTracePreferenceKey()]!=nil),@"disk_trace_level":plist[MJTracePreferenceKey()] ?: NSNull.null,
        @"restore_available":@(backup && MJTraceValidBackup(backup,container)),@"backup_file_present":@(backupExists),
        @"activation_time":backup[@"activation_time"] ?: NSNull.null,@"runtime_effect_verified":@NO};
}
BOOL MJSetTraceRecording(NSString *container, NSString *backupDirectory, BOOL enable, NSError **error) {
    NSString *path=MJTracePath(container,error); if (!path) return NO;
    NSData *snapshot=nil; NSPropertyListFormat format=NSPropertyListBinaryFormat_v1_0;
    NSMutableDictionary *plist=MJTraceRead(path,&snapshot,&format,error); if (!plist) return NO;
    NSString *backupPath=MJTraceBackupPath(container,backupDirectory);
    NSFileManager *fm=NSFileManager.defaultManager;
    BOOL backupExists=[fm fileExistsAtPath:backupPath];
    NSDictionary *backup=backupExists?[NSDictionary dictionaryWithContentsOfFile:backupPath]:nil;
    if (backupExists && !MJTraceValidBackup(backup,container)) return MJTraceFail(error,@"原设置备份不完整，未修改。请保留备份用于检查。");
    id current=plist[MJTracePreferenceKey()];
    if (enable && backupExists) {
        if ([current isEqual:@3]) return YES;
        return MJTraceFail(error,@"保存备份后设置已发生变化；未覆盖现有值。请先核对状态。");
    }
    if (!enable) {
        if (!backupExists) return MJTraceFail(error,@"没有本工具保存的原设置，未修改。");
        BOOL originalMatches=[backup[@"existed"] boolValue]?[current isEqual:backup[@"original"]]:current==nil;
        // A failed earlier write or interrupted restore may have left the original in place.
        if (originalMatches) return [fm removeItemAtPath:backupPath error:error];
        if (![current isEqual:@3]) return MJTraceFail(error,@"设置已被游戏或其他操作更改，未覆盖当前值。");
    }
    if (enable) {
        if (![fm createDirectoryAtPath:backupDirectory withIntermediateDirectories:YES attributes:@{NSFilePosixPermissions:@0700} error:error]) return NO;
        backup=@{@"schema":@1,@"container":container.lastPathComponent,@"key":MJTracePreferenceKey(),@"existed":@(current!=nil),@"original":current ?: NSNull.null,@"activation_time":@([NSDate.date timeIntervalSince1970])};
        // Property lists cannot encode NSNull; absent originals are represented by existed=false.
        NSMutableDictionary *safeBackup=[backup mutableCopy]; if (!current) [safeBackup removeObjectForKey:@"original"];
        NSData *backupData=[NSPropertyListSerialization dataWithPropertyList:safeBackup format:NSPropertyListBinaryFormat_v1_0 options:0 error:error];
        if (!backupData || ![backupData writeToFile:backupPath options:NSDataWritingAtomic error:error]) return NO;
        plist[MJTracePreferenceKey()]=@3;
    } else if ([backup[@"existed"] boolValue]) plist[MJTracePreferenceKey()]=backup[@"original"];
    else [plist removeObjectForKey:MJTracePreferenceKey()];
    if (![[NSData dataWithContentsOfFile:path] isEqual:snapshot]) return MJTraceFail(error,@"设置文件在操作期间发生变化，未写入。请完全关闭游戏后重试。");
    NSDictionary *attributes=[fm attributesOfItemAtPath:path error:error]; if (!attributes) return NO;
    NSData *updated=[NSPropertyListSerialization dataWithPropertyList:plist format:format options:0 error:error];
    if (!updated || ![updated writeToFile:path options:NSDataWritingAtomic error:error]) return NO;
    NSMutableDictionary *preservedAttributes=[NSMutableDictionary dictionary];
    if (attributes[NSFilePosixPermissions]) preservedAttributes[NSFilePosixPermissions]=attributes[NSFilePosixPermissions];
#if TARGET_OS_IPHONE
    if (attributes[NSFileProtectionKey]) preservedAttributes[NSFileProtectionKey]=attributes[NSFileProtectionKey];
#endif
    if (preservedAttributes.count && ![fm setAttributes:preservedAttributes ofItemAtPath:path error:error]) return NO;
    NSDictionary *verified=[NSDictionary dictionaryWithContentsOfFile:path];
    if (![verified isEqual:plist]) return MJTraceFail(error,@"写入后核对未通过；原设置备份已保留。");
    if (!enable && ![fm removeItemAtPath:backupPath error:error]) return NO;
    return YES;
}
