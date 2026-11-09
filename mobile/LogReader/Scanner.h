#import <Foundation/Foundation.h>
FOUNDATION_EXPORT BOOL MJMatchesBundle(NSString *identifier);
FOUNDATION_EXPORT BOOL MJIsCandidate(NSString *relative);
FOUNDATION_EXPORT NSArray<NSString *> *MJFindContainers(NSString *parent, NSMutableArray<NSString *> *errors);
FOUNDATION_EXPORT NSDictionary *MJScanContainer(NSString *container);
FOUNDATION_EXPORT NSDictionary *MJFindAppVersion(NSString *parent);
FOUNDATION_EXPORT NSData *MJCreateZip(NSDictionary<NSString *, NSData *> *files, NSError **error);
FOUNDATION_EXPORT NSDictionary *MJPlanCompleteSelection(NSArray<NSDictionary *> *files);
FOUNDATION_EXPORT NSDictionary *MJExportCoverage(NSDictionary *report, NSArray<NSDictionary *> *files, NSIndexSet *selection, NSArray<NSString *> *exported, NSArray<NSDictionary *> *skipped);
