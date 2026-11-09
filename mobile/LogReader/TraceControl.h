#import <Foundation/Foundation.h>
NSString *MJTracePreferenceKey(void);
NSDictionary *MJTraceStatus(NSString *container, NSString *backupDirectory, NSError **error);
BOOL MJSetTraceRecording(NSString *container, NSString *backupDirectory, BOOL enable, NSError **error);
