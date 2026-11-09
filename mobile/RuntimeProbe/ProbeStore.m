#import "ProbeStore.h"
#import <sys/stat.h>
#import <sys/file.h>
#import <fcntl.h>
#import <unistd.h>
#import <errno.h>
#import <string.h>

#ifndef MJ_PROBE_DIRECTORY_NAME
#define MJ_PROBE_DIRECTORY_NAME "MJRuntimeProbe"
#endif
#ifndef MJ_PROBE_FILE_NAME
#define MJ_PROBE_FILE_NAME "runtime-probe.log"
#endif
#ifndef MJ_PROBE_HEADER
#define MJ_PROBE_HEADER "MJRuntimeProbe/1\n"
#endif

BOOL MJProbeMatchesBundle(NSString *identifier) {
    return [identifier isKindOfClass:NSString.class] && [identifier isEqualToString:@"com.tencent.happymj"];
}
static BOOL MJProbeFailure(NSError **error, NSString *message) {
    if (error) *error=[NSError errorWithDomain:@"MJRuntimeProbe" code:1 userInfo:@{NSLocalizedDescriptionKey:message}];
    return NO;
}
static BOOL MJProbeWriteAll(int fd, const void *data, size_t length) {
    const unsigned char *bytes=data;
    while (length) {
        ssize_t n=write(fd,bytes,length);
        if (n<0 && errno==EINTR) continue;
        if (n<=0) return NO;
        bytes+=n; length-=(size_t)n;
    }
    return YES;
}
BOOL MJProbeAppendRecord(NSString *documents, NSDictionary *record, NSError **error) {
    NSData *json=[NSJSONSerialization dataWithJSONObject:record options:NSJSONWritingSortedKeys error:error];
    if (!json || json.length>16*1024) return MJProbeFailure(error,@"Invalid or oversized probe record");
    NSMutableData *line=[json mutableCopy]; [line appendBytes:"\n" length:1];
    int root=open(documents.fileSystemRepresentation,O_RDONLY|O_DIRECTORY|O_CLOEXEC);
    if (root<0) return MJProbeFailure(error,@"Probe documents directory unavailable");
    if (mkdirat(root,MJ_PROBE_DIRECTORY_NAME,0700)!=0 && errno!=EEXIST) { close(root);return MJProbeFailure(error,@"Probe directory creation failed"); }
    int dir=openat(root,MJ_PROBE_DIRECTORY_NAME,O_RDONLY|O_DIRECTORY|O_NOFOLLOW|O_CLOEXEC);close(root);
    if (dir<0) return MJProbeFailure(error,@"Probe directory is not a regular directory");
    int fd=openat(dir,MJ_PROBE_FILE_NAME,O_RDWR|O_APPEND|O_CREAT|O_NOFOLLOW|O_CLOEXEC,0600);close(dir);
    if (fd<0) return MJProbeFailure(error,@"Probe file unavailable");
    if (flock(fd,LOCK_EX|LOCK_NB)!=0) { close(fd);return MJProbeFailure(error,@"Probe file busy"); }
    struct stat st; const char header[]=MJ_PROBE_HEADER; size_t headerSize=sizeof(header)-1;
    if (fstat(fd,&st)!=0 || !S_ISREG(st.st_mode) || st.st_nlink!=1 || st.st_size<0 || (unsigned long long)st.st_size+line.length+headerSize>64*1024) {
        close(fd);return MJProbeFailure(error,@"Probe file type or size limit");
    }
    if (st.st_size) {
        char existing[sizeof(header)]={0};
        if (pread(fd,existing,headerSize,0)!=(ssize_t)headerSize || memcmp(existing,header,headerSize)!=0) {
            close(fd);return MJProbeFailure(error,@"Existing file is not this probe's log");
        }
    } else if (!MJProbeWriteAll(fd,header,headerSize)) { close(fd);return MJProbeFailure(error,@"Probe header write failed"); }
    BOOL ok=MJProbeWriteAll(fd,line.bytes,line.length);close(fd);
    return ok?YES:MJProbeFailure(error,@"Probe record write failed");
}
