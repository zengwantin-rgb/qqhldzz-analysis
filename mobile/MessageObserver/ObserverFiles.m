#import "ObserverFiles.h"
#import "../RuntimeProbe/ProbeStore.h"
#import <sys/stat.h>
#import <fcntl.h>
#import <unistd.h>
#import <errno.h>
static BOOL MJObserverWrite(int fd, NSData *data) {
    const uint8_t *bytes=data.bytes;size_t left=data.length;
    while(left) { ssize_t n=write(fd,bytes,left);if(n<0 && errno==EINTR)continue;if(n<=0)return NO;bytes+=n;left-=(size_t)n; }
    return YES;
}
static BOOL MJObserverFileError(NSError **error) {
    if(error)*error=[NSError errorWithDomain:@"MJMessageObserver" code:1 userInfo:@{NSLocalizedDescriptionKey:@"Diagnostic file preparation failed"}];
    return NO;
}
BOOL MJObserverPrepareFiles(NSString *documents, NSData *assembly, NSString **assemblyPath, NSString **eventPath, NSString **statusPath, NSError **error) {
    if(!assembly || assembly.length==0 || assembly.length>1024*1024)return MJObserverFileError(error);
    // Status writer supplies the same symlink and existing-content guards as the probe.
    if(!MJProbeAppendRecord(documents,@{@"kind":@"native_status",@"schema":@1,@"status":@"preparing_files"},error))return NO;
    int root=open(documents.fileSystemRepresentation,O_RDONLY|O_DIRECTORY|O_CLOEXEC);
    if(root<0)return MJObserverFileError(error);
    int dir=openat(root,"MJMessageObserver",O_RDONLY|O_DIRECTORY|O_NOFOLLOW|O_CLOEXEC);close(root);
    if(dir<0)return MJObserverFileError(error);
    NSString *nonce=NSUUID.UUID.UUIDString;
    NSString *dll=[NSString stringWithFormat:@"managed-%@.dll",nonce];
    NSString *events=[NSString stringWithFormat:@"game-events-%@.log",nonce];
    int fd=openat(dir,dll.fileSystemRepresentation,O_WRONLY|O_CREAT|O_EXCL|O_NOFOLLOW|O_CLOEXEC,0600);
    if(fd<0){close(dir);return MJObserverFileError(error);}
    BOOL ok=MJObserverWrite(fd,assembly);close(fd);
    if(!ok){unlinkat(dir,dll.fileSystemRepresentation,0);close(dir);return MJObserverFileError(error);}
    fd=openat(dir,events.fileSystemRepresentation,O_WRONLY|O_CREAT|O_EXCL|O_NOFOLLOW|O_CLOEXEC,0600);
    if(fd<0){unlinkat(dir,dll.fileSystemRepresentation,0);close(dir);return MJObserverFileError(error);}
    ok=MJObserverWrite(fd,[@"MJMessageObserverEvents/1\n" dataUsingEncoding:NSUTF8StringEncoding]);close(fd);
    if(!ok){unlinkat(dir,events.fileSystemRepresentation,0);unlinkat(dir,dll.fileSystemRepresentation,0);close(dir);return MJObserverFileError(error);}
    close(dir);
    NSString *folder=[documents stringByAppendingPathComponent:@"MJMessageObserver"];
    if(assemblyPath)*assemblyPath=[folder stringByAppendingPathComponent:dll];
    if(eventPath)*eventPath=[folder stringByAppendingPathComponent:events];
    if(statusPath)*statusPath=[folder stringByAppendingPathComponent:@"observer-status.log"];
    return YES;
}
