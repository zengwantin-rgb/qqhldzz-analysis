"""Shared read-only bundle directory primitives."""
import struct
def readz(b,p):
    e=b.index(b'\0',p)
    return b[p:e].decode(errors='replace'),e+1

def valid_info(b):
    try:
        n=struct.unpack_from('>I',b,16)[0]
        if not 0<n<100000: return None
        p=20
        blocks=[]
        for i in range(n):
            u,c,f=struct.unpack_from('>IIH',b,p);p+=10
            if not (0<u<100000000 and 0<c<100000000):return None
            blocks.append({'uncompressed':u,'compressed':c,'flags':f})
        count=struct.unpack_from('>I',b,p)[0];p+=4
        if not 0<count<10000:return None
        nodes=[]
        for i in range(count):
            off,size,flags=struct.unpack_from('>QQI',b,p);p+=20
            name,p=readz(b,p)
            if not name or len(name)>512 or any(ord(ch)<32 for ch in name):return None
            nodes.append({'offset':off,'size':size,'flags':flags,'name':name})
        if p!=len(b):return None
        return {'blocks':blocks,'nodes':nodes}
    except (struct.error, ValueError):return None
