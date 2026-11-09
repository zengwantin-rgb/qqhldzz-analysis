#import "ProbeStore.h"
static void Check(BOOL condition,NSString *message) { if (!condition) { NSLog(@"FAIL: %@",message);exit(1); } }
int main(void) { @autoreleasepool {
    NSFileManager *fm=NSFileManager.defaultManager;
    NSString *root=[NSTemporaryDirectory() stringByAppendingPathComponent:NSUUID.UUID.UUIDString];
    [fm createDirectoryAtPath:root withIntermediateDirectories:YES attributes:nil error:NULL];
    NSString *folder=[root stringByAppendingPathComponent:@"MJRuntimeProbe"],*file=[folder stringByAppendingPathComponent:@"runtime-probe.log"];
    NSDictionary *record=@{@"kind":@"MJRuntimeProbe",@"stage":@"fixture",@"capturing_game_messages":@NO};
    Check(MJProbeMatchesBundle(@"com.tencent.happymj") && !MJProbeMatchesBundle(@"com.tencent.happymj.clone") && !MJProbeMatchesBundle((id)@1),@"Only the exact application is eligible");
    NSString *original=[root stringByAppendingPathComponent:@"original.log"];
    [@"unchanged fixture" writeToFile:original atomically:YES encoding:NSUTF8StringEncoding error:NULL];
    Check(MJProbeAppendRecord(root,record,NULL),@"Create probe log");
    Check(MJProbeAppendRecord(root,record,NULL),@"Append probe log");
    NSString *text=[NSString stringWithContentsOfFile:file encoding:NSUTF8StringEncoding error:NULL];
    Check([text hasPrefix:@"MJRuntimeProbe/1\n"] && [text componentsSeparatedByString:@"\n"].count==4,@"Exactly one header and two records");
    Check([[fm attributesOfItemAtPath:file error:NULL][NSFilePosixPermissions] unsignedIntegerValue]==0600,@"New log has private file permissions");
    Check([[NSString stringWithContentsOfFile:original encoding:NSUTF8StringEncoding error:NULL] isEqual:@"unchanged fixture"],@"Unrelated game files are unchanged");
    [fm removeItemAtPath:file error:NULL];
    [@"unrelated contents" writeToFile:file atomically:YES encoding:NSUTF8StringEncoding error:NULL];
    NSData *before=[NSData dataWithContentsOfFile:file];
    Check(!MJProbeAppendRecord(root,record,NULL) && [[NSData dataWithContentsOfFile:file] isEqual:before],@"Do not modify a same-name unrelated file");
    [fm removeItemAtPath:file error:NULL];
    [fm createSymbolicLinkAtPath:file withDestinationPath:original error:NULL];
    Check(!MJProbeAppendRecord(root,record,NULL),@"Reject file symlink");
    [fm removeItemAtPath:folder error:NULL];
    NSString *outside=[root stringByAppendingPathComponent:@"outside"];
    [fm createDirectoryAtPath:outside withIntermediateDirectories:YES attributes:nil error:NULL];
    [fm createSymbolicLinkAtPath:folder withDestinationPath:outside error:NULL];
    Check(!MJProbeAppendRecord(root,record,NULL) && [fm contentsOfDirectoryAtPath:outside error:NULL].count==0,@"Reject directory symlink");
    [fm removeItemAtPath:folder error:NULL];
    Check(MJProbeAppendRecord(root,record,NULL),@"Create log before cap test");
    NSFileHandle *handle=[NSFileHandle fileHandleForWritingAtPath:file];[handle truncateFileAtOffset:64*1024];[handle closeFile];
    before=[NSData dataWithContentsOfFile:file];
    Check(!MJProbeAppendRecord(root,record,NULL) && [[NSData dataWithContentsOfFile:file] isEqual:before],@"Cap preserves existing file without truncation");
    Check(!MJProbeAppendRecord([root stringByAppendingPathComponent:@"missing"],record,NULL),@"Missing documents root is not created");
    [fm removeItemAtPath:root error:NULL];
    puts("PASS: exact bundle, private append-only log, existing content guard, file/directory symlinks and size limit");
    return 0;
} }
