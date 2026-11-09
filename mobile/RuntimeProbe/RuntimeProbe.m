#import <Foundation/Foundation.h>
#import <dispatch/dispatch.h>
#import <dlfcn.h>
#import <mach-o/dyld.h>
#import <string.h>
#import <stdlib.h>
#import "ProbeStore.h"

// Opaque metadata handles only. No game objects, method bodies or field values are read.
typedef void *(*MJDomainGet)(void);
typedef void *(*MJThreadCurrent)(void);
typedef void *(*MJThreadAttach)(void *);
typedef void (*MJThreadDetach)(void *);
typedef const void **(*MJGetAssemblies)(void *,size_t *);
typedef const void *(*MJAssemblyImage)(const void *);
typedef const char *(*MJImageName)(const void *);
typedef void *(*MJClassFromName)(const void *,const char *,const char *);
typedef const void *(*MJClassMethods)(void *,void **);
typedef const char *(*MJMethodName)(const void *);
typedef uint32_t (*MJParamCount)(const void *);
typedef void *(*MJClassField)(void *,const char *);

static const char *MJApis[]={
    "il2cpp_domain_get","il2cpp_thread_current","il2cpp_thread_attach","il2cpp_thread_detach",
    "il2cpp_domain_get_assemblies","il2cpp_assembly_get_image","il2cpp_image_get_name","il2cpp_class_from_name",
    "il2cpp_class_get_methods","il2cpp_method_get_name","il2cpp_method_get_param_count","il2cpp_class_get_field_from_name",
    "il2cpp_class_get_method_from_name","il2cpp_field_static_get_value","il2cpp_runtime_invoke","il2cpp_object_new",
    "il2cpp_array_new","il2cpp_gchandle_new","il2cpp_get_corlib","il2cpp_method_get_param","il2cpp_type_get_type",
    "il2cpp_class_from_type","il2cpp_class_get_element_class","il2cpp_object_unbox","il2cpp_string_new"
};
static void *MJFind(void *handle,const char *name) {
    void *address=dlsym(RTLD_DEFAULT,name);
    return address ?: (handle?dlsym(handle,name):NULL);
}
static void MJStore(NSDictionary *record) {
    NSString *documents=[NSSearchPathForDirectoriesInDomains(NSDocumentDirectory,NSUserDomainMask,YES) firstObject];
    if (documents) MJProbeAppendRecord(documents,record,NULL);
}
static void *MJUnityHandle(BOOL *loaded) {
    *loaded=NO;
    uint32_t count=_dyld_image_count();
    for (uint32_t i=0;i<count;i++) {
        const char *path=_dyld_get_image_name(i);
        if (!path) continue;
        NSString *name=[NSString stringWithUTF8String:path];
        if (![name hasSuffix:@"/UnityFramework.framework/UnityFramework"]) continue;
        *loaded=YES;
        // Obtain an already-loaded handle; do not load a new executable image.
        return dlopen(path,RTLD_LAZY|RTLD_NOLOAD);
    }
    return NULL;
}
static NSDictionary *MJMethods(void *klass,MJClassMethods next,MJMethodName name,MJParamCount count) {
    NSMutableArray *matches=[NSMutableArray array];
    NSSet *allowed=[NSSet setWithArray:@[@"get_dispatcher",@"AddListener",@"RemoveListener",@"Dispatch",@"Invoke",@"Load",@"get_data"]];
    void *iterator=NULL; const void *method=NULL; NSUInteger scanned=0;
    while (scanned<256 && (method=next(klass,&iterator))) {
        scanned++;
        const char *raw=name(method); if (!raw) continue;
        NSString *methodName=[NSString stringWithUTF8String:raw];
        if (![allowed containsObject:methodName]) continue;
        uint32_t parameters=count(method);
        if (parameters<=32) [matches addObject:@{@"name":methodName,@"parameter_count":@(parameters)}];
    }
    return @{@"methods":matches,@"method_scan_capped":@(scanned==256)};
}
static void MJSnapshot(NSUInteger attempt) {
    @autoreleasepool {
        if (!MJProbeMatchesBundle(NSBundle.mainBundle.bundleIdentifier)) return;
        NSMutableDictionary *record=[@{@"kind":@"MJRuntimeProbe",@"schema":@1,@"probe_version":@"0.1.0",
            @"stage":@"runtime_snapshot",@"attempt":@(attempt),@"capturing_game_messages":@NO} mutableCopy];
        BOOL unityLoaded=NO; void *handle=MJUnityHandle(&unityLoaded);
        record[@"unity_framework_loaded"]=@(unityLoaded);
        NSMutableDictionary *symbols=[NSMutableDictionary dictionary];
        for (size_t i=0;i<sizeof(MJApis)/sizeof(MJApis[0]);i++) {
            BOOL available=MJFind(handle,MJApis[i])!=NULL;
            symbols[[NSString stringWithUTF8String:MJApis[i]]]=@(available);
        }
        record[@"native_apis"]=symbols;
        MJDomainGet domainGet=(MJDomainGet)MJFind(handle,"il2cpp_domain_get");
        MJThreadCurrent current=(MJThreadCurrent)MJFind(handle,"il2cpp_thread_current");
        MJThreadAttach attach=(MJThreadAttach)MJFind(handle,"il2cpp_thread_attach");
        MJThreadDetach detach=(MJThreadDetach)MJFind(handle,"il2cpp_thread_detach");
        MJGetAssemblies assemblies=(MJGetAssemblies)MJFind(handle,"il2cpp_domain_get_assemblies");
        MJAssemblyImage image=(MJAssemblyImage)MJFind(handle,"il2cpp_assembly_get_image");
        MJImageName imageName=(MJImageName)MJFind(handle,"il2cpp_image_get_name");
        MJClassFromName findClass=(MJClassFromName)MJFind(handle,"il2cpp_class_from_name");
        MJClassMethods methods=(MJClassMethods)MJFind(handle,"il2cpp_class_get_methods");
        MJMethodName methodName=(MJMethodName)MJFind(handle,"il2cpp_method_get_name");
        MJParamCount paramCount=(MJParamCount)MJFind(handle,"il2cpp_method_get_param_count");
        MJClassField field=(MJClassField)MJFind(handle,"il2cpp_class_get_field_from_name");
        if (!domainGet || !current || !attach || !detach || !assemblies || !image || !imageName || !findClass || !methods || !methodName || !paramCount || !field) {
            record[@"lookup_status"]=@"required_api_unavailable";
            MJStore(record);if(handle)dlclose(handle);return;
        }
        void *domain=domainGet();
        if (!domain) { record[@"lookup_status"]=@"domain_not_ready";MJStore(record);if(handle)dlclose(handle);return; }
        void *previousThread=current();
        void *thread=previousThread ?: attach(domain);
        if (!thread) { record[@"lookup_status"]=@"thread_attach_failed";MJStore(record);if(handle)dlclose(handle);return; }
        size_t total=0;const void **all=assemblies(domain,&total);
        size_t limit=total>512?512:total;
        const void **snapshot=limit && all?calloc(limit,sizeof(void *)):NULL;
        if (limit && (!all || !snapshot)) {
            record[@"lookup_status"]=@"assembly_snapshot_unavailable";
            if(!previousThread)detach(thread);MJStore(record);if(handle)dlclose(handle);return;
        }
        if (snapshot) memcpy(snapshot,all,limit*sizeof(void *));
        record[@"assembly_count"]=@(total);record[@"assembly_scan_capped"]=@((BOOL)(total>512));
        const struct {const char *space;const char *name;const char *expectedImage;} targets[]={
            {"HappyMahjong","TSDK4CSharpService","HappyMahjong.Hall"},
            {"HappyMahjong","TSDKMessageEvent","HappyMahjong.Hall"},
            {"strange.extensions.dispatcher.eventdispatcher.api","IEventDispatcher","HappyMahjong.Base"},
            {"strange.extensions.dispatcher.eventdispatcher.api","EventCallback","HappyMahjong.Base"},
            {"strange.extensions.dispatcher.eventdispatcher.api","IEvent","HappyMahjong.Base"},
            {"strange.extensions.dispatcher.eventdispatcher.impl","EventDispatcher","HappyMahjong.Base"},
            {"System.Reflection","Assembly","mscorlib"}
        };
        NSMutableArray *classes=[NSMutableArray array];
        for (size_t j=0;j<sizeof(targets)/sizeof(targets[0]);j++) {
            NSMutableDictionary *row=[@{@"type":[NSString stringWithFormat:@"%s.%s",targets[j].space,targets[j].name],@"found":@NO} mutableCopy];
            for (size_t i=0;i<limit;i++) {
                if (!snapshot[i]) continue;
                const void *img=image(snapshot[i]);if(!img)continue;
                void *klass=findClass(img,targets[j].space,targets[j].name);if(!klass)continue;
                row[@"found"]=@YES;
                const char *imgName=imageName(img);
                NSString *actual=imgName?[NSString stringWithUTF8String:imgName]:@"";
                NSString *expected=[NSString stringWithUTF8String:targets[j].expectedImage];
                row[@"expected_image"]=@((BOOL)([actual isEqual:expected] || [actual isEqual:[expected stringByAppendingString:@".dll"]]));
                [row addEntriesFromDictionary:MJMethods(klass,methods,methodName,paramCount)];
                if (j==0) row[@"dispatcher_backing_field_found"]=@((BOOL)(field(klass,"<dispatcher>k__BackingField")!=NULL));
                if (j==1) {
                    row[@"game_message_field_found"]=@((BOOL)(field(klass,"OnTSDKGameMessage")!=NULL));
                    row[@"game_pb_message_field_found"]=@((BOOL)(field(klass,"OnTSDKGamePbMessage")!=NULL));
                }
                break;
            }
            [classes addObject:row];
        }
        free(snapshot);
        record[@"classes"]=classes;record[@"lookup_status"]=@"metadata_checked";
        if (!previousThread) detach(thread);
        MJStore(record);if(handle)dlclose(handle);
    }
}
__attribute__((constructor)) static void MJRuntimeProbeStart(void) {
    @autoreleasepool {
        if (!MJProbeMatchesBundle(NSBundle.mainBundle.bundleIdentifier)) return;
        dispatch_async(dispatch_get_main_queue(),^{
            @try {
                MJStore(@{@"kind":@"MJRuntimeProbe",@"schema":@1,@"probe_version":@"0.1.0",@"stage":@"library_loaded",@"capturing_game_messages":@NO});
                const NSUInteger delays[]={10,30,60};
                for (NSUInteger i=0;i<3;i++) {
                    NSUInteger attempt=i+1;
                    dispatch_after(dispatch_time(DISPATCH_TIME_NOW,(int64_t)(delays[i]*NSEC_PER_SEC)),dispatch_get_main_queue(),^{
                        @try { MJSnapshot(attempt); }
                        @catch (NSException *exception) { MJStore(@{@"kind":@"MJRuntimeProbe",@"schema":@1,@"stage":@"objective_c_error",@"attempt":@(attempt),@"capturing_game_messages":@NO}); }
                    });
                }
            } @catch (NSException *exception) { /* No UI or original-file changes on failure. */ }
        });
    }
}
