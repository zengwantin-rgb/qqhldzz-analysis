#import "Scanner.h"
#import <zlib.h>

BOOL MJMatchesBundle(NSString *identifier) {
    return [identifier isEqualToString:@"com.tencent.happymj"];
}

BOOL MJIsCandidate(NSString *relative) {
    NSString *path = relative.lowercaseString;
    for (NSString *word in @[@"cookie", @"credential", @"keychain", @"token", @"accounts", @"webkit"])
        if ([path containsString:word]) return NO;
    if ([path containsString:@"/tracebinlogs/"] || [path hasPrefix:@"tracebinlogs/"]) return YES;
    NSString *name = path.lastPathComponent;
    NSString *ext = name.pathExtension;
    if ([@[@"log", @"xlog", @"clog", @"replay", @"trace"] containsObject:ext]) return YES;
    if ([ext isEqualToString:@"bin"] && ([path containsString:@"trace"] || [path containsString:@"log"])) return YES;
    if (([path containsString:@"replay"] || [path containsString:@"record"]) && [@[@"data", @"xml", @"json", @"txt", @"dat", @"bytes"] containsObject:ext]) return YES;
    if ([@[@"mjgamebaseconfig.data", @"modulefanconfig.data", @"svrnotifytips.data"] containsObject:name]) return YES;
    if ([path containsString:@"patch"] && [@[@"patch", @"bytes", @"bin", @"dll"] containsObject:ext]) return YES;
    return NO;
}

NSArray<NSString *> *MJFindContainers(NSString *parent, NSMutableArray<NSString *> *errors) {
    NSError *error = nil;
    NSArray *children = [[NSFileManager defaultManager] contentsOfDirectoryAtPath:parent error:&error];
    if (!children) { [errors addObject:error.localizedDescription ?: @"无法读取应用容器目录"]; return @[]; }
    NSMutableArray *found = [NSMutableArray array];
    for (NSString *name in children) {
        NSString *path = [parent stringByAppendingPathComponent:name];
        NSNumber *isLink = nil;
        [[NSURL fileURLWithPath:path] getResourceValue:&isLink forKey:NSURLIsSymbolicLinkKey error:nil];
        if (isLink.boolValue) continue;
        NSDictionary *metadata = [NSDictionary dictionaryWithContentsOfFile:[path stringByAppendingPathComponent:@".com.apple.mobile_container_manager.metadata.plist"]];
        id identifier = metadata[@"MCMMetadataIdentifier"];
        if ([identifier isKindOfClass:NSString.class] && MJMatchesBundle(identifier)) [found addObject:path];
    }
    return [found sortedArrayUsingSelector:@selector(compare:)];
}

NSDictionary *MJScanContainer(NSString *container) {
    NSURL *root = [NSURL fileURLWithPath:container isDirectory:YES];
    NSString *prefix = [container stringByAppendingString:@"/"];
    NSMutableArray *files = [NSMutableArray array];
    NSMutableArray *errors = [NSMutableArray array];
    NSArray *keys = @[NSURLIsDirectoryKey, NSURLIsRegularFileKey, NSURLIsSymbolicLinkKey, NSURLFileSizeKey, NSURLContentModificationDateKey];
    NSDirectoryEnumerator<NSURL *> *enumerator = [[NSFileManager defaultManager] enumeratorAtURL:root includingPropertiesForKeys:keys options:NSDirectoryEnumerationSkipsHiddenFiles errorHandler:^BOOL(NSURL *url, NSError *error) {
        if (errors.count < 20) [errors addObject:[NSString stringWithFormat:@"%@: %@", url.lastPathComponent, error.localizedDescription]];
        return YES;
    }];
    NSUInteger visited = 0;
    BOOL truncated = NO;
    for (NSURL *url in enumerator) {
        if (++visited > 50000 || files.count >= 1000) { truncated = YES; break; }
        NSDictionary *values = [url resourceValuesForKeys:keys error:nil];
        if ([values[NSURLIsSymbolicLinkKey] boolValue]) { [enumerator skipDescendants]; continue; }
        if (![values[NSURLIsRegularFileKey] boolValue] || ![url.path hasPrefix:prefix]) continue;
        NSString *relative = [url.path substringFromIndex:prefix.length];
        if (!MJIsCandidate(relative)) continue;
        NSDate *modified = values[NSURLContentModificationDateKey] ?: NSDate.distantPast;
        [files addObject:@{@"container":container, @"relative":relative, @"path":url.path,
                           @"size":values[NSURLFileSizeKey] ?: @0, @"modified":@([modified timeIntervalSince1970])}];
    }
    [files sortUsingComparator:^NSComparisonResult(NSDictionary *a, NSDictionary *b) { return [b[@"modified"] compare:a[@"modified"]]; }];
    return @{@"container":container, @"files":files, @"visited":@(visited), @"truncated":@(truncated), @"errors":errors};
}

NSDictionary *MJFindAppVersion(NSString *parent) {
    NSFileManager *fm = NSFileManager.defaultManager;
    for (NSString *outer in [fm contentsOfDirectoryAtPath:parent error:nil]) {
        NSString *directory = [parent stringByAppendingPathComponent:outer];
        for (NSString *inner in [fm contentsOfDirectoryAtPath:directory error:nil]) {
            if (![inner.pathExtension isEqualToString:@"app"]) continue;
            NSString *app = [directory stringByAppendingPathComponent:inner];
            NSDictionary *info = [NSDictionary dictionaryWithContentsOfFile:[app stringByAppendingPathComponent:@"Info.plist"]];
            if (MJMatchesBundle(info[@"CFBundleIdentifier"]))
                return @{@"bundle_id":info[@"CFBundleIdentifier"], @"version":info[@"CFBundleShortVersionString"] ?: @"", @"build":info[@"CFBundleVersion"] ?: @""};
        }
    }
    return @{};
}

static void MJ16(NSMutableData *d, uint16_t n) { uint8_t b[]={n&255,(n>>8)&255}; [d appendBytes:b length:2]; }
static void MJ32(NSMutableData *d, uint32_t n) { uint8_t b[]={n&255,(n>>8)&255,(n>>16)&255,(n>>24)&255}; [d appendBytes:b length:4]; }
NSData *MJCreateZip(NSDictionary<NSString *, NSData *> *files, NSError **error) {
    NSMutableData *zip = [NSMutableData data];
    NSMutableData *central = [NSMutableData data];
    NSUInteger total = 0;
    if (files.count > 1001) goto invalid;
    for (NSString *name in [files.allKeys sortedArrayUsingSelector:@selector(compare:)]) {
        NSData *payload = files[name];
        NSData *utf8 = [name dataUsingEncoding:NSUTF8StringEncoding];
        if ([name hasPrefix:@"/"] || [[name componentsSeparatedByString:@"/"] containsObject:@".."] || utf8.length > 65535 || payload.length > 20*1024*1024) goto invalid;
        total += payload.length;
        if (total > 64*1024*1024) goto invalid;
        uint32_t size=(uint32_t)payload.length, crc=(uint32_t)crc32(0,payload.bytes,(uInt)payload.length), offset=(uint32_t)zip.length;
        MJ32(zip,0x04034b50); MJ16(zip,20); MJ16(zip,0x800); MJ16(zip,0); MJ16(zip,0); MJ16(zip,0x21);
        MJ32(zip,crc); MJ32(zip,size); MJ32(zip,size); MJ16(zip,(uint16_t)utf8.length); MJ16(zip,0);
        [zip appendData:utf8]; [zip appendData:payload];
        MJ32(central,0x02014b50); MJ16(central,20); MJ16(central,20); MJ16(central,0x800); MJ16(central,0); MJ16(central,0); MJ16(central,0x21);
        MJ32(central,crc); MJ32(central,size); MJ32(central,size); MJ16(central,(uint16_t)utf8.length);
        MJ16(central,0); MJ16(central,0); MJ16(central,0); MJ16(central,0); MJ32(central,0); MJ32(central,offset); [central appendData:utf8];
    }
    { uint32_t offset=(uint32_t)zip.length;
      [zip appendData:central]; MJ32(zip,0x06054b50); MJ16(zip,0); MJ16(zip,0); MJ16(zip,(uint16_t)files.count); MJ16(zip,(uint16_t)files.count); MJ32(zip,(uint32_t)central.length); MJ32(zip,offset); MJ16(zip,0); }
    return zip;
invalid:
    if (error) *error=[NSError errorWithDomain:@"MJLogReader" code:1 userInfo:@{NSLocalizedDescriptionKey:@"导出文件超过限制，或文件名不合法。请减少选择的文件。"}];
    return nil;
}
