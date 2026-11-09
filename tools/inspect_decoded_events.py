"""Publish only counters from privately decoded protocol objects, never tile values."""
import collections,json,pathlib,re,zlib

def proto_fields(raw):
    index=0;counts=collections.Counter()
    def varint():
        nonlocal index
        value=0
        for shift in range(0,70,7):
            if index>=len(raw):raise ValueError()
            byte=raw[index];index+=1;value|=(byte&127)<<shift
            if not byte&128:return value
        raise ValueError()
    while index<len(raw):
        key=varint();tag,wire=key>>3,key&7
        if not 0<tag<536870912:raise ValueError()
        counts[str(tag)]+=1
        if sum(counts.values())>100000:raise ValueError()
        if wire==0:varint()
        elif wire==1:index+=8
        elif wire==2:
            length=varint();index+=length
        elif wire==5:index+=4
        else:raise ValueError()
        if index>len(raw):raise ValueError()
    return dict(counts)

def embedded(value):
    count,original,cards=(value.get(k) for k in ('nGameBuf','iOriginSize','szGameBuf'))
    if type(count) is not int or not 0<count<=32767 or type(original) is not int or not 0<original<=4*1024*1024 or not isinstance(cards,list) or len(cards)<count or not all(type(c) is int and -128<=c<=255 for c in cards):return {'status':'invalid_buffer_shape'}
    raw=bytes(c&255 for c in cards[:count])
    for window in (15,-15,31):
        try:
            stream=zlib.decompressobj(window);clear=stream.decompress(raw,4*1024*1024+1)
            if len(clear)!=original or not stream.eof or stream.unconsumed_tail or stream.unused_data:continue
            tags=proto_fields(clear)
            return {'status':'decompressed_proto_fields','compression':{15:'zlib',-15:'raw_deflate',31:'gzip'}[window],'compressed_bytes':count,'decoded_bytes':len(clear),'top_level_field_counts':tags,'only_known_advice_field':set(tags)=={'1'}}
        except (ValueError,zlib.error):continue
    return {'status':'not_decoded','compressed_bytes':count,'reported_origin_bytes':original}

def inspect(directory):
    frames=[]
    for path in sorted(pathlib.Path(directory).glob('entry-*-seq-*.tdr.json')):
        match=re.fullmatch(r'entry-(\d+)-seq-(\d+)\.tdr\.json',path.name)
        if not match:continue
        data=json.loads(path.read_text());rows=[];waits=[];draws=[];buffers=[];tlvs=collections.Counter()
        def walk(value):
            if isinstance(value,dict):
                if type(value.get('nType')) is int and 'nLen' in value:tlvs[str(value['nType'])]+=1
                if all(k in value for k in ('nSeat','nHandTilesNum','szTiles')) and type(value['nSeat']) is int and 0<=value['nSeat']<=3:
                    cards=value['szTiles'];count=value['nHandTilesNum']
                    if isinstance(cards,list) and type(count) is int and 0<=count<=32 and all(type(c) is int for c in cards):
                        used=cards[:count]
                        rows.append({'seat':value['nSeat'],'declared_tiles':count,'array_length':len(cards),'nonzero_tiles':sum(c!=0 for c in used),'zero_tiles':sum(c==0 for c in used),'length_agrees':len(cards)==count})
                if all(k in value for k in ('chChair','chExCmd','nExtraTilesNum')):
                    cards=value.get('ExtraTiles',[]);count=value['nExtraTilesNum'];seat=value['chChair']
                    if type(seat) is int and 0<=seat<=3 and type(count) is int and 0<=count<=4096 and isinstance(cards,list) and all(type(c) is int for c in cards):waits.append({'seat':seat,'extra_command_nonzero':value['chExCmd']!=0,'declared_extra_tiles':count,'array_length':len(cards),'nonzero_extra_tiles':sum(c!=0 for c in cards[:count])})
                if all(k in value for k in ('nSeatID','nDrawTileNum','chTile')) and type(value['nSeatID']) is int and 0<=value['nSeatID']<=3:
                    cards=value.get('DrawTiles',[]);count=value['nDrawTileNum']
                    if type(count) is int and 0<=count<=32 and isinstance(cards,list) and all(type(c) is int for c in cards):draws.append({'seat':value['nSeatID'],'single_tile_nonzero':value['chTile']!=0,'declared_draw_tiles':count,'array_length':len(cards),'nonzero_draw_tiles':sum(c!=0 for c in cards[:count])})
                if all(k in value for k in ('nGameBuf','iOriginSize','szGameBuf')):buffers.append(embedded(value))
                for v in value.values():walk(v)
            elif isinstance(value,list):
                for v in value:walk(v)
        walk(data)
        frames.append({'entry':int(match[1]),'sequence':int(match[2]),'message_type':data.get('iType'),'hand_fields':rows,'wait_fields':waits,'draw_fields':draws,'embedded_proto_buffers':buffers,'tlv_type_counts':dict(tlvs)})
    states={}
    for frame in frames:
        entry=frame['entry'];phase=states.get(entry,'before_hu')
        if frame['message_type'] in (50,51,54):phase='after_hu_or_finish';states[entry]=phase
        frame['phase']=phase
    return {'decoded_frame_count':len(frames),'frames':frames,'limits':['Card values, identifiers, raw payloads and private paths are never emitted.','Only active union branches are traversed by the decoder. Hidden display is not established by message counts.','Seat identity must be reconciled independently; post-Hu hands do not establish live visibility.','Phase is relative to captured Hu/finish only; a missing earlier Hu cannot be inferred absent.']}
if __name__=='__main__':pathlib.Path('report/device/decoded-gameplay-counts.json').write_text(json.dumps(inspect('private-output'),indent=2)+'\n')
