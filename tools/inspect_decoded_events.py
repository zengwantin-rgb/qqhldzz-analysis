"""Publish only counters from privately decoded protocol objects, never tile values."""
import collections,json,pathlib,re

def inspect(directory):
    frames=[]
    for path in sorted(pathlib.Path(directory).glob('entry-*-seq-*.tdr.json')):
        match=re.fullmatch(r'entry-(\d+)-seq-(\d+)\.tdr\.json',path.name)
        if not match:continue
        data=json.loads(path.read_text());rows=[];waits=[];tlvs=collections.Counter()
        def walk(value):
            if isinstance(value,dict):
                if type(value.get('nType')) is int and 'nLen' in value:tlvs[str(value['nType'])]+=1
                if all(k in value for k in ('nSeat','nHandTilesNum','szTiles')) and type(value['nSeat']) is int and 0<=value['nSeat']<=3:
                    cards=value['szTiles'];count=value['nHandTilesNum']
                    if isinstance(cards,list) and type(count) is int and 0<=count<=32 and all(type(c) is int for c in cards):
                        used=cards[:count]
                        rows.append({'seat':value['nSeat'],'declared_tiles':count,'array_length':len(cards),'nonzero_tiles':sum(c!=0 for c in used),'zero_tiles':sum(c==0 for c in used),'length_agrees':len(cards)==count})
                if all(k in value for k in ('chChair','chExCmd','nExtraTilesNum','ExtraTiles')):
                    cards=value['ExtraTiles'];count=value['nExtraTilesNum'];seat=value['chChair']
                    if type(seat) is int and 0<=seat<=3 and type(count) is int and 0<=count<=4096 and isinstance(cards,list) and all(type(c) is int for c in cards):waits.append({'seat':seat,'extra_command_nonzero':value['chExCmd']!=0,'declared_extra_tiles':count,'array_length':len(cards),'nonzero_extra_tiles':sum(c!=0 for c in cards[:count])})
                for v in value.values():walk(v)
            elif isinstance(value,list):
                for v in value:walk(v)
        walk(data)
        frames.append({'entry':int(match[1]),'sequence':int(match[2]),'message_type':data.get('iType'),'hand_fields':rows,'wait_fields':waits,'tlv_type_counts':dict(tlvs)})
    states={}
    for frame in frames:
        entry=frame['entry'];phase=states.get(entry,'before_hu')
        if frame['message_type'] in (50,51,54):phase='after_hu_or_finish';states[entry]=phase
        frame['phase']=phase
    return {'decoded_frame_count':len(frames),'frames':frames,'limits':['Card values, identifiers, raw payloads and private paths are never emitted.','Only active union branches are traversed by the decoder. Hidden display is not established by message counts.','Seat identity must be reconciled independently; post-Hu hands do not establish live visibility.','Phase is relative to captured Hu/finish only; a missing earlier Hu cannot be inferred absent.']}
if __name__=='__main__':pathlib.Path('report/device/decoded-gameplay-counts.json').write_text(json.dumps(inspect('private-output'),indent=2)+'\n')
