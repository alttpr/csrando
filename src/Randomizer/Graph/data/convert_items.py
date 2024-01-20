import sys
import yaml

def modify_yaml(input_file, output_file):
    # Read the input YAML file
    with open(input_file, 'r') as file:
        data = yaml.safe_load(file)

    # Change from:
    #      
    # L1Sword:
    #   bytes: [0x00]
    #
    # to:
    #
    # L1Sword:
    #   bytes:
    #     z1: [0x00]
    #     z3: [0x00]
    #     m1: [0x00]
    #     m3: [0x00]
        
    # where each key is a different game where the item data format is different
    # we need to take the format without the game-specific data and change it to it

    # For each item in the input file
    for item in data:
        # Get the bytes for the item
        bytes = data[item]['bytes']
        # Remove the bytes from the item
        data[item].pop('bytes')
        # Add the modified bytes to the item for each game
        data[item]['bytes'] = {
            'z1': [byte + 0x30 if byte < 0xD0 else byte - 0xD0 for byte in bytes],
            'z3': bytes,
            'm1': bytes,
            'm3': bytes
        }        

    # Make sure the yaml output doesn't use references
    yaml.Dumper.ignore_aliases = lambda *args : True

    # and write out integers as hex
    yaml.add_representer(int, lambda dumper, data: dumper.represent_int(hex(data)))

    # inline arrays
    yaml.add_representer(list, lambda dumper, data: dumper.represent_sequence(u'tag:yaml.org,2002:seq', data, flow_style=True))

    # when writing the file, sort the items by z3 item id (z3 bytes) (and length of byte array, shortest first)    
    data = dict(sorted(data.items(), key=lambda item: (len(item[1]['bytes']['z3']), item[1]['bytes']['z3'], )))
    
    yaml.add_representer(dict, lambda dumper, data: dumper.represent_mapping(u'tag:yaml.org,2002:map', data.items(), flow_style=False))

    # Write the modified YAML data to the output file
    with open(output_file, 'w') as file:
        yaml.dump(data, file)

if __name__ == "__main__":
    # Check if the input and output file paths are provided
    if len(sys.argv) < 3:
        print("Usage: python convert_items.py <input_file> <output_file>")
        sys.exit(1)

    input_file = sys.argv[1]
    output_file = sys.argv[2]

    modify_yaml(input_file, output_file)
