import struct

def read_file(file_path):
    with open(file_path, 'rb') as file:
        return file.read()

def test_values(data, offset):
    types = {
        'int8': 'b',
        'uint8': 'B',
        'int16': 'h',
        'uint16': 'H',
        'int32': 'i',
        'uint32': 'I',
        'int64': 'q',
        'uint64': 'Q',
        'float32': 'f',
        'float64': 'd',
    }
    
    for type_name, fmt in types.items():
        size = struct.calcsize(fmt)
        if offset + size <= len(data):
            value = struct.unpack_from(fmt, data, offset)[0]
            print(f"Offset {offset}: {type_name} = {value}")

def bit_tester(file_path):
    data = read_file(file_path)
    for offset in range(20):
        print(f"Testing at offset {offset}:")
        test_values(data, offset)
        print("-" * 40)

# Usage
file_path = "F:\Leaks\Genshin\Tools\ExcelParser\ExcelParser\Data\scene3_point"
bit_tester(file_path)