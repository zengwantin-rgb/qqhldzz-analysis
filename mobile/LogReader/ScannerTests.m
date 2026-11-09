#import "Scanner.h"
static void Require(BOOL ok, NSString *message) { if (!ok) { NSLog(@"FAIL: %@",message); exit(1); } }
int main(int argc,char *argv[]) { @autoreleasepool {
    NSFileManager *fm=NSFileManager.defaultManager;
    NSString *root=[NSTemporaryDirectory() stringByAppendingPathComponent:NSUUID.UUID.UUIDString];
    NSString *target=[root stringByAppendingPathComponent:@"target"];
    NSString *other=[root stringByAppendingPathComponent:@"other"];
    for (NSString *dir in @[target,other]) [fm createDirectoryAtPath:[dir stringByAppendingPathComponent:@"Documents/Logs"] withIntermediateDirectories:YES attributes:nil error:NULL];
    [@{@"MCMMetadataIdentifier":@"com.tencent.happymj"} writeToFile:[target stringByAppendingPathComponent:@".com.apple.mobile_container_manager.metadata.plist"] atomically:YES];
    [@{@"MCMMetadataIdentifier":@"com.example.unrelated"} writeToFile:[other stringByAppendingPathComponent:@".com.apple.mobile_container_manager.metadata.plist"] atomically:YES];
    NSData *log=[@"ordinary match diagnostic fixture" dataUsingEncoding:NSUTF8StringEncoding];
    NSString *source=[target stringByAppendingPathComponent:@"Documents/Logs/match.log"];
    [log writeToFile:source atomically:YES];
    [log writeToFile:[target stringByAppendingPathComponent:@"Documents/Logs/token.log"] atomically:YES];
    [fm createSymbolicLinkAtPath:[target stringByAppendingPathComponent:@"Documents/Logs/escape.log"] withDestinationPath:[other stringByAppendingPathComponent:@"private.log"] error:NULL];
    NSMutableArray *errors=[NSMutableArray array];
    NSArray *containers=MJFindContainers(root,errors);
    Require(containers.count==1 && [containers[0] isEqual:target],@"Only the exact target bundle may be scanned");
    NSDictionary *scan=MJScanContainer(target);NSArray *files=scan[@"files"];
    Require(files.count==1 && [files[0][@"relative"] isEqual:@"Documents/Logs/match.log"],@"Skip credentials and symlinks");
    Require([[NSData dataWithContentsOfFile:source] isEqual:log],@"Scanning must not modify the original file");
    Require(!MJMatchesBundle(@"com.tencent.happymj.clone"),@"Do not silently broaden target to cloned bundle IDs");
    Require(MJIsCandidate(@"Library/TraceBin/sample.bin"),@"Trace files should be discoverable");
    Require(!MJIsCandidate(@"Library/Cookies/private.log"),@"Cookie directories must be excluded");
    NSError *error=nil;
    NSData *zip=MJCreateZip(@{@"manifest.json":[@"{}" dataUsingEncoding:NSUTF8StringEncoding], @"files/日志.txt":log},&error);
    Require(zip!=nil,@"ZIP export must succeed");
    Require([zip writeToFile:[NSString stringWithUTF8String:argv[1]] atomically:YES],@"Write ZIP test artifact");
    Require(MJCreateZip(@{@"../escape.txt":log},&error)==nil,@"Reject ZIP path traversal");
    error=nil;
    Require(MJCreateZip(@{@"large.log":[NSMutableData dataWithLength:20*1024*1024+1]},&error)==nil,@"Enforce the per-file export limit");
    [fm removeItemAtPath:root error:NULL];
    puts("PASS: target isolation, sensitive-path filtering, symlink rejection, read-only scan, ZIP creation and bounds");
    return 0;
} }
