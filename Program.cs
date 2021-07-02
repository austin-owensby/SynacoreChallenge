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

            // Reusable variables
            ushort aValue;
            ushort bValue;
            ushort cValue;

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
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the destination register in the set command.");
                        }

                        aValue = memory[programCounter];

                        if(aValue < registerStartAddress || aValue >= stackStartAddress){
                            throw new InvalidOperationException($"{aValue} is an invalid address for the register of the set command. Line {programCounter}.");
                        }

                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the source in the set command.");
                        }

                        bValue = memory[programCounter];

                        if(bValue >= stackStartAddress){
                            throw new InvalidOperationException($"{bValue} is an invalid operation for the set command. Line {programCounter}.");
                        }

                        if(bValue >= registerStartAddress){
                            memory[aValue] = memory[bValue];
                        }
                        else{
                            memory[aValue] = bValue;
                        }
                        
                        memory[programCounterAddress]++;

                        break;
                    case 2: // push: 2 a
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the source in the push command.");
                        }

                        aValue = memory[programCounter];

                        if(aValue >= stackStartAddress){
                            throw new InvalidOperationException($"{aValue} is an invalid operation for the push command. Line {programCounter}.");
                        }

                        ++memory[stackPointerAddress];

                        if(memory[stackPointerAddress] >= stackPointerAddress){
                            throw new StackOverflowException($"Stack overflow while trying to push. Line {programCounter}.");
                        }

                        if(aValue >= registerStartAddress){
                            memory[memory[stackPointerAddress]] = memory[aValue];
                        }
                        else{
                            memory[memory[stackPointerAddress]] = aValue;
                        }
                        
                        memory[programCounterAddress]++;

                        break;
                    case 3: // pop: 3 a
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the destination in the pop command.");
                        }

                        aValue = memory[programCounter];

                        if(aValue >= stackStartAddress){
                            throw new InvalidOperationException($"{aValue} is an invalid address for the pop command. Line {programCounter}.");
                        }

                        if(memory[stackPointerAddress] <= stackStartAddress){
                            throw new InvalidOperationException($"Stack is empty, cannot pop. Line {programCounter}.");
                        }

                        memory[aValue] = memory[--memory[stackPointerAddress]];
                        
                        memory[programCounterAddress]++;

                        break;
                    case 4: // eq 4 a b c
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the destination in the eq command.");
                        }

                        aValue = memory[programCounter];

                        if(aValue >= stackStartAddress){
                            throw new InvalidOperationException($"{aValue} is an invalid address for the eq command. Line {programCounter}.");
                        }

                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the first source in the eq command.");
                        }

                        bValue = memory[programCounter];

                        if(bValue >= stackStartAddress){
                            throw new InvalidOperationException($"{bValue} is an invalid first operation for the eq command. Line {programCounter}.");
                        }
                        
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the second source in the eq command.");
                        }

                        cValue = memory[programCounter];

                        if(cValue >= stackStartAddress){
                            throw new InvalidOperationException($"{cValue} is an invalid second operation for the eq command. Line {programCounter} for the second source register of eq command.");
                        }

                        if(bValue >= registerStartAddress){
                            if(cValue >= registerStartAddress){
                                memory[aValue] = memory[bValue] == memory[cValue] ? (ushort)1 : (ushort)0;
                            }
                            else{
                                memory[aValue] = memory[bValue] == cValue ? (ushort)1 : (ushort)0;
                            }
                        }
                        else if(cValue >= registerStartAddress){
                            memory[aValue] = bValue == memory[cValue] ? (ushort)1 : (ushort)0;
                        }
                        else{
                            memory[aValue] = bValue == cValue ? (ushort)1 : (ushort)0;
                        }
                        
                        memory[programCounterAddress]++;

                        break;
                    case 5: // gt: 5 a b c
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the destination in the gt command.");
                        }

                        aValue = memory[programCounter];

                        if(aValue >= stackStartAddress){
                            throw new InvalidOperationException($"{aValue} is an invalid address for the gt command. Line {programCounter}.");
                        }

                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the first source in the gt command.");
                        }

                        bValue = memory[programCounter];

                        if(bValue >= stackStartAddress){
                            throw new InvalidOperationException($"{bValue} is an invalid first operation for the gt command. Line {programCounter}.");
                        }
                        
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the second source in the gt command.");
                        }

                        cValue = memory[programCounter];

                        if(cValue >= stackStartAddress){
                            throw new InvalidOperationException($"{cValue} is an invalid second operation for the gt command. Line {programCounter}.");
                        }

                        if(bValue >= registerStartAddress){
                            if(cValue >= registerStartAddress){
                                memory[aValue] = memory[bValue] > memory[cValue] ? (ushort)1 : (ushort)0;
                            }
                            else{
                                memory[aValue] = memory[bValue] > cValue ? (ushort)1 : (ushort)0;
                            }
                        }
                        else if(cValue >= registerStartAddress){
                            memory[aValue] = bValue > memory[cValue] ? (ushort)1 : (ushort)0;
                        }
                        else{
                            memory[aValue] = bValue > cValue ? (ushort)1 : (ushort)0;
                        }
                        
                        memory[programCounterAddress]++;

                        break;
                    case 6: // jmp: 6 a
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the destination for the jmp command.");
                        }

                        aValue = memory[programCounter];

                        if(aValue >= stackStartAddress){
                            throw new InvalidOperationException($"{aValue} is an invalid address for the jmp command. Line {programCounter}.");
                        }

                        if(aValue >= registerStartAddress && memory[aValue] >= registerStartAddress){
                            throw new InvalidOperationException($"{memory[aValue]} is an invalid address for the jmp command. Line {programCounter}.");
                        }

                        if(aValue >= registerStartAddress){
                            memory[programCounterAddress] = memory[aValue];
                        }
                        else{
                            memory[programCounterAddress] = aValue;
                        }

                        break;
                    case 7: // jt: 7 a b
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the conditional in the jt command.");
                        }

                        aValue = memory[programCounter];

                        if(aValue >= stackStartAddress){
                            throw new InvalidOperationException($"{aValue} is an invalid operation for the jt command. Line {programCounter}.");
                        }

                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the destination in the jt command.");
                        }

                        bValue = memory[programCounter];

                        if(bValue >= stackStartAddress){
                            throw new InvalidOperationException($"{bValue} is an invalid address for the destination of the jt command. Line {programCounter}.");
                        }

                        if(bValue >= registerStartAddress && memory[bValue] >= registerStartAddress){
                            throw new InvalidOperationException($"{memory[bValue]} is an invalid address for the destination of the jt command. Line {programCounter}.");
                        }

                        if(aValue >= registerStartAddress){
                            if(memory[aValue] != 0){
                                if(bValue >= registerStartAddress){
                                    memory[programCounterAddress] = memory[bValue];
                                }
                                else{
                                    memory[programCounterAddress] = bValue;
                                }
                            }
                            else{
                                memory[programCounterAddress]++;
                            }
                        }
                        else{
                            if(aValue != 0){
                                if(bValue >= registerStartAddress){
                                    memory[programCounterAddress] = memory[bValue];
                                }
                                else{
                                    memory[programCounterAddress] = bValue;
                                }
                            }
                            else{
                                memory[programCounterAddress]++;
                            }
                        }

                        break;
                    case 8: // jf: 8 a b
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the conditional in the jf command.");
                        }

                        aValue = memory[programCounter];

                        if(aValue >= stackStartAddress){
                            throw new InvalidOperationException($"{aValue} is an invalid operation for the jf command. Line {programCounter}.");
                        }

                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the destination in the jf command.");
                        }

                        bValue = memory[programCounter];

                        if(bValue >= stackStartAddress){
                            throw new InvalidOperationException($"{bValue} is an invalid address for the destination of the jf command. Line {programCounter}.");
                        }

                        if(bValue >= registerStartAddress && memory[bValue] >= registerStartAddress){
                            throw new InvalidOperationException($"{memory[bValue]} is an invalid address for the destination of the jf command. Line {programCounter}.");
                        }

                        if(aValue >= registerStartAddress){
                            if(memory[aValue] == 0){
                                if(bValue >= registerStartAddress){
                                    memory[programCounterAddress] = memory[bValue];
                                }
                                else{
                                    memory[programCounterAddress] = bValue;
                                }
                            }
                            else{
                                memory[programCounterAddress]++;
                            }
                        }
                        else{
                            if(aValue == 0){
                                if(bValue >= registerStartAddress){
                                    memory[programCounterAddress] = memory[bValue];
                                }
                                else{
                                    memory[programCounterAddress] = bValue;
                                }
                            }else{
                                memory[programCounterAddress]++;
                            }
                        }
                                                
                        break;
                    case 9: // add: 9 a b c
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the destination in the add command.");
                        }

                        aValue = memory[programCounter];

                        if(aValue >= stackStartAddress){
                            throw new InvalidOperationException($"{aValue} is an invalid address for the add command. Line {programCounter}.");
                        }

                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the first source in the add command.");
                        }

                        bValue = memory[programCounter];

                        if(bValue >= stackStartAddress){
                            throw new InvalidOperationException($"{bValue} is an invalid first operation for the add command. Line {programCounter}.");
                        }
                        
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the second source in the add command.");
                        }

                        cValue = memory[programCounter];

                        if(cValue >= stackStartAddress){
                            throw new InvalidOperationException($"{cValue} is an invalid second operation for the add command. Line {programCounter}.");
                        }
                        
                        if(bValue >= registerStartAddress){
                            if(cValue >= registerStartAddress){
                                memory[aValue] = (ushort)((memory[bValue] + memory[cValue]) % registerStartAddress);
                            }
                            else{
                                memory[aValue] = (ushort)((memory[bValue] + cValue) % registerStartAddress);
                            }
                        }
                        else{
                            memory[aValue] = (ushort)((bValue + cValue) % registerStartAddress);
                        }
                        
                        memory[programCounterAddress]++;

                        break;
                    case 10: // mult: a b c
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the destination in the mult command.");
                        }

                        aValue = memory[programCounter];

                        if(aValue >= stackStartAddress){
                            throw new InvalidOperationException($"{aValue} is an invalid address for the mult command. Line {programCounter}.");
                        }

                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the first source in the mult command.");
                        }

                        bValue = memory[programCounter];

                        if(bValue >= stackStartAddress){
                            throw new InvalidOperationException($"{bValue} is an invalid first operation for the mult command. Line {programCounter}.");
                        }
                        
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the second source in the mult command.");
                        }

                        cValue = memory[programCounter];

                        if(cValue >= stackStartAddress){
                            throw new InvalidOperationException($"{cValue} is an invalid second operation for the mult command. Line {programCounter}.");
                        }
                        
                        if(bValue >= registerStartAddress){
                            if(cValue >= registerStartAddress){
                                memory[aValue] = (ushort)((memory[bValue] * memory[cValue]) % registerStartAddress);
                            }
                            else{
                                memory[aValue] = (ushort)((memory[bValue] * cValue) % registerStartAddress);
                            }
                        }
                        else{
                            memory[aValue] = (ushort)((bValue * cValue) % registerStartAddress);
                        }
                        
                        memory[programCounterAddress]++;

                        break;
                    case 11: // mod 11 a b c
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the destination in the mod command.");
                        }

                        aValue = memory[programCounter];

                        if(aValue >= stackStartAddress){
                            throw new InvalidOperationException($"{aValue} is an invalid address for the mod command. Line {programCounter}.");
                        }

                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the first source in the mod command.");
                        }

                        bValue = memory[programCounter];

                        if(bValue >= stackStartAddress){
                            throw new InvalidOperationException($"{bValue} is an invalid first operation for the mod command. Line {programCounter}.");
                        }
                        
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the second source in the mod command.");
                        }

                        cValue = memory[programCounter];

                        if(cValue >= stackStartAddress){
                            throw new InvalidOperationException($"{cValue} is an invalid second operation for the mod command. Line {programCounter}.");
                        }
                        
                        if(bValue >= registerStartAddress){
                            if(cValue >= registerStartAddress){
                                memory[aValue] = (ushort)(memory[bValue] % memory[cValue]);
                            }
                            else{
                                memory[aValue] = (ushort)(memory[bValue] % cValue);
                            }
                        }
                        else{
                            memory[aValue] = (ushort)(bValue % cValue);
                        }
                        
                        memory[programCounterAddress]++;

                        break;
                    case 12: // and: 12 a b c
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the destination in the and command.");
                        }

                        aValue = memory[programCounter];

                        if(aValue >= stackStartAddress){
                            throw new InvalidOperationException($"{aValue} is an invalid address for the and command. Line {programCounter}.");
                        }

                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the first source in the and command.");
                        }

                        bValue = memory[programCounter];

                        if(bValue >= stackStartAddress){
                            throw new InvalidOperationException($"{bValue} is an invalid first operation for the and command. Line {programCounter}.");
                        }
                        
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the second source in the and command.");
                        }

                        cValue = memory[programCounter];

                        if(cValue >= stackStartAddress){
                            throw new InvalidOperationException($"{cValue} is an invalid second operation for the and command. Line {programCounter}.");
                        }
                        
                        if(bValue >= registerStartAddress){
                            if(cValue >= registerStartAddress){
                                memory[aValue] = (ushort)(memory[bValue] & memory[cValue]);
                            }
                            else{
                                memory[aValue] = (ushort)(memory[bValue] & cValue);
                            }
                        }
                        else{
                            memory[aValue] = (ushort)(bValue & cValue);
                        }
                        
                        memory[programCounterAddress]++;

                        break;
                    case 13: // or: 13 a b c
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the destination in the or command.");
                        }

                        aValue = memory[programCounter];

                        if(aValue >= stackStartAddress){
                            throw new InvalidOperationException($"{aValue} is an invalid address for the or command. Line {programCounter}.");
                        }

                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the first source in the or command.");
                        }

                        bValue = memory[programCounter];

                        if(bValue >= stackStartAddress){
                            throw new InvalidOperationException($"{bValue} is an invalid first operation for the or command. Line {programCounter}.");
                        }
                        
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the second source in the or command.");
                        }

                        cValue = memory[programCounter];

                        if(cValue >= stackStartAddress){
                            throw new InvalidOperationException($"{cValue} is an invalid second operation for the or command. Line {programCounter}.");
                        }
                        
                        if(bValue >= registerStartAddress){
                            if(cValue >= registerStartAddress){
                                memory[aValue] = (ushort)(memory[bValue] | memory[cValue]);
                            }
                            else{
                                memory[aValue] = (ushort)(memory[bValue] | cValue);
                            }
                        }
                        else{
                            memory[aValue] = (ushort)(bValue | cValue);
                        }
                        
                        memory[programCounterAddress]++;
                        
                        break;
                    case 14: // not: 14 a b
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the destination in the not command.");
                        }

                        aValue = memory[programCounter];

                        if(aValue >= stackStartAddress){
                            throw new InvalidOperationException($"{aValue} is an invalid address for the not command. Line {programCounter}.");
                        }

                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the first source in the not command.");
                        }

                        bValue = memory[programCounter];

                        if(bValue >= stackStartAddress){
                            throw new InvalidOperationException($"{bValue} is an invalid first operation for the not command. Line {programCounter}.");
                        }
                        
                        if(bValue >= registerStartAddress){
                            memory[aValue] = (ushort)(~memory[bValue]);
                        }
                        else{
                            memory[aValue] = (ushort)(~bValue);
                        }
                        
                        memory[programCounterAddress]++;
                        
                        break;
                    case 15: // rmem: 15 a b
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the destination in the rmem command.");
                        }

                        aValue = memory[programCounter];

                        if(aValue >= stackStartAddress){
                            throw new InvalidOperationException($"{aValue} is an invalid address for the rmem command. Line {programCounter}.");
                        }

                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the first source in the rmem command.");
                        }

                        bValue = memory[programCounter];

                        if(bValue >= stackStartAddress){
                            throw new InvalidOperationException($"{bValue} is an invalid first operation for the rmem command. Line {programCounter}.");
                        }
                        
                        if(bValue >= registerStartAddress){
                            if(memory[bValue] >= registerStartAddress){
                                throw new InvalidOperationException("${memory[bValue]} is an invalid memory address to read from the rmem command. Line {programCounter}.");
                            }
                            
                            memory[aValue] = memory[memory[bValue]];
                        }
                        else{
                            if(bValue >= registerStartAddress){
                                throw new InvalidOperationException("${bValue} is an invalid memory address to read from the rmem command. Line {programCounter}.");
                            }

                            memory[aValue] = memory[bValue];
                        }
                        
                        memory[programCounterAddress]++;

                        break;
                    case 16: // wrem: 16 a b
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the destination in the wmem command.");
                        }

                        aValue = memory[programCounter];

                        if(aValue >= stackStartAddress){
                            throw new InvalidOperationException($"{aValue} is an invalid address for the wmem command. Line {programCounter}.");
                        }

                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the first source in the wmem command.");
                        }

                        bValue = memory[programCounter];

                        if(bValue >= stackStartAddress){
                            throw new InvalidOperationException($"{bValue} is an invalid first operation for the wmem command. Line {programCounter}.");
                        }
                        
                        if(bValue >= registerStartAddress){
                            if(memory[aValue] >= registerStartAddress){
                                throw new InvalidOperationException("${memory[aValue]} is an invalid memory address to write to the wmem command. Line {programCounter}.");
                            }
                            
                            memory[memory[aValue]] = memory[bValue];
                        }
                        else{
                            if(aValue >= registerStartAddress){
                                throw new InvalidOperationException("${aValue} is an invalid memory address to write to the wmem command. Line {programCounter}.");
                            }

                            memory[aValue] = bValue;
                        }
                        
                        memory[programCounterAddress]++;

                        break;
                    case 17: // call: 17 a
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the destination in the call command.");
                        }

                        aValue = memory[programCounter];

                        if(aValue >= stackStartAddress){
                            throw new InvalidOperationException($"{aValue} is an invalid address for the call command. Line {programCounter}.");
                        }

                        ++memory[stackPointerAddress];

                        if(memory[stackPointerAddress] >= stackPointerAddress){
                            throw new StackOverflowException($"Stack overflow while trying to call. Line {programCounter}.");
                        }

                        if(aValue >= registerStartAddress){
                            memory[memory[stackPointerAddress]] = memory[programCounter + 1];
                            memory[programCounterAddress] = memory[aValue];
                        }
                        else{
                            memory[programCounterAddress] = aValue;
                        }

                        break;
                    case 18: // ret: 18
                        if(memory[stackPointerAddress] <= stackStartAddress){
                            throw new InvalidOperationException($"Stack is empty, cannot ret. Line {programCounter}.");
                        }

                        memory[programCounterAddress] = memory[--memory[stackPointerAddress]];

                        break;
                    case 19: // out: 19 a
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to output a value to the console.");
                        }

                        aValue = memory[programCounter];
                        
                        var test = Convert.ToChar(aValue);

                        Console.Write(Convert.ToChar(aValue));
                        
                        memory[programCounterAddress]++;
                        break;
                    case 20: // in: 20 a
                        programCounter = ++memory[programCounterAddress];

                        if(programCounter >= registerStartAddress){
                            throw new IndexOutOfRangeException($"The program counter {programCounter} is pointing to an address out of memory while trying to get the destination in the in command.");
                        }

                        aValue = memory[programCounter];

                        if(aValue >= stackStartAddress){
                            throw new InvalidOperationException($"{aValue} is an invalid address for the in command. Line {programCounter}.");
                        }

                        byte[] input = Encoding.ASCII.GetBytes(Console.ReadLine());
                        if(input.Length > 2){
                            throw new InvalidOperationException($"Input stream is too long for in command. Line {programCounter}");
                        }

                        memory[aValue] = BitConverter.ToUInt16(input);

                        memory[programCounterAddress]++;

                        break;
                    case 21: // noop: 21
                        memory[programCounterAddress]++;
                        break;
                    default:
                        throw new InvalidOperationException($"{instruction} is not a valid operation. Line {programCounter}.");
                }
            }
        }
    }
}
