#import "TraceControl.h"
static void Check(BOOL ok,NSString *message) { if (!ok) { NSLog(@"FAIL: %@",message); exit(1); } }
int main(void) { @autoreleasepool {
    NSFileManager *fm=NSFileManager.defaultManager;
    NSString *root=[[NSTemporaryDirectory() stringByAppendingPathComponent:NSUUID.UUID.UUIDString] stringByResolvingSymlinksInPath];
    NSString *container=[root stringByAppendingPathComponent:@"game"],*backups=[root stringByAppendingPathComponent:@"backups"];
    NSString *prefs=[container stringByAppendingPathComponent:@"Library/Preferences/com.tencent.happymj.plist"];
    [fm createDirectoryAtPath:prefs.stringByDeletingLastPathComponent withIntermediateDirectories:YES attributes:nil error:NULL];
    NSString *metadata=[container stringByAppendingPathComponent:@".com.apple.mobile_container_manager.metadata.plist"];
    [@{@"MCMMetadataIdentifier":@"com.tencent.happymj"} writeToFile:metadata atomically:YES];
    NSString *key=MJTracePreferenceKey();
    NSDictionary *original=@{@"unrelated_fixture":@"preserve me",@"another":@42};
    [original writeToFile:prefs atomically:YES];
    NSError *error=nil;
    NSDictionary *state=MJTraceStatus(container,backups,&error);
    if (!state) NSLog(@"Fixture status error: %@",error);
    Check(state && ![state[@"preference_present"] boolValue],@"Absent override must be distinct from explicit off");
    Check(MJSetTraceRecording(container,backups,YES,&error),@"Enable missing override");
    Check([[NSDictionary dictionaryWithContentsOfFile:prefs][key] isEqual:@3],@"Exact trace setting written");
    NSString *backup=[backups stringByAppendingPathComponent:@"game-trace.plist"];
    NSDictionary *saved=[NSDictionary dictionaryWithContentsOfFile:backup];
    Check(saved && !saved[@"unrelated_fixture"] && !saved[@"another"],@"Backup contains no unrelated preferences");
    Check(MJSetTraceRecording(container,backups,YES,&error),@"Repeated enable preserves original backup");
    NSMutableDictionary *changed=[[NSDictionary dictionaryWithContentsOfFile:prefs] mutableCopy];changed[@"another"]=@43;[changed writeToFile:prefs atomically:YES];
    Check(MJSetTraceRecording(container,backups,NO,&error),@"Restore absent original");
    NSDictionary *restored=[NSDictionary dictionaryWithContentsOfFile:prefs];
    Check(!restored[key] && [restored[@"another"] isEqual:@43] && [restored[@"unrelated_fixture"] isEqual:@"preserve me"],@"Restore only our key and preserve subsequent unrelated changes");
    for (NSNumber *value in @[@(-1),@0,@1,@2,@3]) {
        [@{key:value,@"another":@42} writeToFile:prefs atomically:YES];
        Check(MJSetTraceRecording(container,backups,YES,&error),@"Enable existing integer override");
        Check(MJSetTraceRecording(container,backups,NO,&error),@"Restore original integer override");
        Check([[NSDictionary dictionaryWithContentsOfFile:prefs][key] isEqual:value],@"Exact original integer restored");
    }
    [original writeToFile:prefs atomically:YES];
    Check(MJSetTraceRecording(container,backups,YES,&error),@"Enable before external change");
    changed=[[NSDictionary dictionaryWithContentsOfFile:prefs] mutableCopy];changed[key]=@1;[changed writeToFile:prefs atomically:YES];
    error=nil;Check(!MJSetTraceRecording(container,backups,NO,&error) && error,@"Refuse to overwrite externally changed setting");
    Check([[NSDictionary dictionaryWithContentsOfFile:prefs][key] isEqual:@1],@"External value preserved");
    changed[key]=@3;[changed writeToFile:prefs atomically:YES];Check(MJSetTraceRecording(container,backups,NO,NULL),@"Can restore after resolving conflict");
    for (id bad in @[@YES,@4,@1.5,@"3"]) {
        [@{key:bad} writeToFile:prefs atomically:YES];
        NSData *before=[NSData dataWithContentsOfFile:prefs];
        Check(!MJSetTraceRecording(container,backups,YES,NULL),@"Reject unexpected diagnostic value types");
        Check([[NSData dataWithContentsOfFile:prefs] isEqual:before],@"Rejected values leave original bytes unchanged");
    }
    [original writeToFile:prefs atomically:YES];
    [@{@"MCMMetadataIdentifier":@"com.tencent.happymj.clone"} writeToFile:metadata atomically:YES];
    Check(!MJSetTraceRecording(container,backups,YES,NULL),@"Reject different app bundle");
    [@{@"MCMMetadataIdentifier":@"com.tencent.happymj"} writeToFile:metadata atomically:YES];
    NSString *outside=[root stringByAppendingPathComponent:@"outside.plist"];[original writeToFile:outside atomically:YES];
    [fm removeItemAtPath:prefs error:NULL];[fm createSymbolicLinkAtPath:prefs withDestinationPath:outside error:NULL];
    Check(!MJSetTraceRecording(container,backups,YES,NULL),@"Reject preference file symlink");
    Check([[NSDictionary dictionaryWithContentsOfFile:outside] isEqual:original],@"Symlink target unchanged");
    [fm removeItemAtPath:prefs error:NULL];
    Check(!MJSetTraceRecording(container,backups,YES,NULL),@"Do not invent missing game preference file");
    [original writeToFile:prefs atomically:YES];
    [fm createDirectoryAtPath:backup withIntermediateDirectories:YES attributes:nil error:NULL];
    NSData *before=[NSData dataWithContentsOfFile:prefs];
    Check(!MJSetTraceRecording(container,backups,YES,NULL),@"Refuse invalid backup and leave original untouched");
    Check([[NSData dataWithContentsOfFile:prefs] isEqual:before],@"Backup failure cannot modify game setting");
    [fm removeItemAtPath:root error:NULL];
    puts("PASS: exact app/key, scoped backups, absent/integer restoration, idempotency, unrelated edits, conflicts, unexpected types, symlinks, missing files and backup failures");
    return 0;
} }
