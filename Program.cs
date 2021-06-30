using System.Text;
using System;
using System.IO;

namespace SynacoreChallenge
{
    class Program
    {
        const int totalMemory = 65536; // 2^16
        const ushort programCounterAddress = (ushort)(totalMemory - 1); // 2^16 - 1
        const ushort stackPointerAddress = (ushort)(totalMemory - 2); // 2^16 - 2
        const ushort registerStartAddress = (ushort)32768; // 2^15
        const ushort stackStartAddress = (ushort)(registerStartAddress + 8); // 2^15 + 8

        static void Main(string[] args)
        {
            // Read binary file contents
            byte[] data = File.ReadAllBytes(Path.Combine(Environment.CurrentDirectory,"challenge.bin"));

            // Store contents into memory
            ushort[] memory = new ushort[totalMemory];
            Buffer.BlockCopy(data, 0, memory, 0, data.Length);

            // Initialize the stack pointer (address 2^16 - 2) to 2^15 + 8
            memory[stackPointerAddress] = stackStartAddress;

            while(true){
                uint programCounter = memory[programCounterAddress];
                
                if(programCounter >= registerStartAddress){
                    throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory.");
                }

                uint instruction = memory[programCounter];

                switch (instruction)
                {
                    case 0: // halt: 0
                        return;
                    case 1: // set: 1 a b
                        break;
                    case 2: // push: 2 a
                        break;
                    case 3: // pop: 3 a
                        break;
                    case 4: // eq 4 a b c
                        break;
                    case 5: // gt: 5 a b c
                        break;
                    case 6: // jmp: 6 a
                        break;
                    case 7: // jt: 7 a b
                        break;
                    case 8: // jf: 8 a b
                        break;
                    case 9: // add: 9 a b c
                        break;
                    case 10: // mult: a b c
                        break;
                    case 11: // mod 11 a b c
                        break;
                    case 12: // and: 12 a b c
                        break;
                    case 13: // or: 13 a b c
                        break;
                    case 14: // not: 14 a b
                        break;
                    case 15: // rmem: 15 a b
                        break;
                    case 16: // wrem: 16 a b
                        break;
                    case 17: // call: 17 a
                        break;
                    case 18: // ret: 18
                        break;
                    case 19: // out: 19 a
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to output a value to the console.");
                        }

                        uint output = memory[programCounter];
                        
                        Console.Write(Encoding.ASCII.GetString(BitConverter.GetBytes(output)));
                        break;
                    case 20: // in: 20 a
                        break;
                    case 21: // noop: 21
                        break;
                    default:
                        throw new InvalidOperationException($"{instruction} is not a valid operation. Line {programCounter}.");
                }

                // Finally increase the program counter
                memory[programCounterAddress]++;
            }
        }
    }
}
