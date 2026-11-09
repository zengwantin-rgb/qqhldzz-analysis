#import "ObserverFiles.h"
#import "../RuntimeProbe/ProbeStore.h"
static void Check(BOOL value,NSString *message){if(!value){NSLog(@"FAIL: %@",message);exit(1);}}
int main(void){@autoreleasepool{
    NSFileManager *fm=NSFileManager.defaultManager;NSString *root=[NSTemporaryDirectory() stringByAppendingPathComponent:NSUUID.UUID.UUIDString];
    [fm createDirectoryAtPath:root withIntermediateDirectories:YES attributes:nil error:NULL];
    NSData *payload=[@"MZ private fixture" dataUsingEncoding:NSUTF8StringEncoding];NSString *dll=nil,*events=nil,*status=nil;
    Check(MJObserverPrepareFiles(root,payload,&dll,&events,&status,NULL),@"Prepare independent payload and event files");
    Check([[NSData dataWithContentsOfFile:dll] isEqual:payload],@"Embedded payload bytes preserved");
    Check([[NSString stringWithContentsOfFile:events encoding:NSUTF8StringEncoding error:NULL] isEqual:@"MJMessageObserverEvents/1\n"],@"Event journal starts with its format header");
    Check([[fm attributesOfItemAtPath:events error:NULL][NSFilePosixPermissions] unsignedIntegerValue]==0600,@"Event file permissions private");
    NSString *oldDll=dll,*oldEvents=events;
    Check(MJObserverPrepareFiles(root,payload,&dll,&events,&status,NULL) && ![dll isEqual:oldDll] && ![events isEqual:oldEvents],@"Each bootstrap gets unique files without overwriting earlier captures");
    Check([[NSData dataWithContentsOfFile:oldDll] isEqual:payload],@"Previous payload unchanged");
    [fm removeItemAtPath:root error:NULL];
    [fm createDirectoryAtPath:root withIntermediateDirectories:YES attributes:nil error:NULL];
    NSString *outside=[root stringByAppendingPathComponent:@"outside"];[fm createDirectoryAtPath:outside withIntermediateDirectories:YES attributes:nil error:NULL];
    [fm createSymbolicLinkAtPath:[root stringByAppendingPathComponent:@"MJMessageObserver"] withDestinationPath:outside error:NULL];
    Check(!MJObserverPrepareFiles(root,payload,&dll,&events,&status,NULL),@"Reject output-directory symlink");
    Check([fm contentsOfDirectoryAtPath:outside error:NULL].count==0,@"Symlink target unchanged");
    [fm removeItemAtPath:root error:NULL];
    puts("PASS: embedded payload, private event files, unique-session paths, original-file preservation and symlink refusal");
    return 0;
}}
