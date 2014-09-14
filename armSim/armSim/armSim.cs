using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;


public class armsim{

	//Comment for commit
	public static StreamWriter log;
	public static Options optionParser;

	public static void printArray(byte[] array){
		string output = "";

		output = "The Array\n";
		for (int i = 0; i < array.Length; i++) {
			output += array[i].ToString("X2");
			i++;
			output += array [i].ToString ("X2") + " ";
			if ((i+1) % 16 == 0){
					output += "\n";
			}
		}
		log.WriteLine( output);

	}

	/*
     * 
	 * Unused might be good in the future
	public static byte[] stringToByteArray(string input){

		byte[] bA = new byte[input.Length * sizeof(char)];

		char[] inputArray = input.ToCharArray ();
		System.Buffer.BlockCopy(inputArray, 0, bA, 0, bA.Length);
		return bA;
	}
	*/

	public static int run(string[] args)
	{

		optionParser = new Options ();



		try{
            Console.WriteLine("We are here");
		    log = new StreamWriter("log.txt");
		    log.WriteLine ("Log: Start");

		    if (optionParser.parse (args)) {
			    if (optionParser.getTest ()) {
				    TestRam.RunTests (log);
				    TestArmSim.RunTests (log);
			    }

			    log.WriteLine ("Log: MemSize " + optionParser.getMemSize());
			    log.WriteLine ("Log: File " + optionParser.getFile());
			    log.WriteLine ("Log: Little Endian " + BitConverter.IsLittleEndian);


			    ELFReader e = new ELFReader ();

			    byte[] elfArray = File.ReadAllBytes (optionParser.getFile ());

			    e.ReadHeader (elfArray);



			    log.WriteLine ("ELF: Header Position " + e.elfHeader.e_phoff);
			    log.WriteLine ("ELF: Header Size " + e.elfHeader.e_ehsize );
			    log.WriteLine ("ELF: Entry Position " + e.elfHeader.e_entry.ToString ("X4"));
			    log.WriteLine ("ELF: Number of program headers " + e.elfHeader.e_phnum);

			    for (int i = 1; i <= e.elfHeader.e_phnum; i++) {
				    log.WriteLine ("ELF: Program Header {0}, Offset = {1}, Size = {2}",
				                   i,
				                   e.elfHeader.e_phoff,
				                   e.elfHeader.e_phentsize);
			    }

			    //ignore the entry point for a loader
			    //its for (executing)going into the ram after it's loaded
			
			    ramSim ram = new ramSim (optionParser.getMemSize());
			    writeElfToRam (e, elfArray, ref ram);


			    log.WriteLine("Log: Ram Hash " + ram.getHash ());


			    printArray(ram.getArray ());

		}
		} catch{

			log.WriteLine ("Log: Something went wrong");

            log.Close();
            return -1;
		}

        //all is good in the world,the sky is blue, the tank is clean...THE TANK IS CLEAN!!
		log.WriteLine ("Log: Program Finished");
		log.WriteLine ("-----------------------------");
		log.Close ();

		return 0;
	}

	public static void writeElfToRam(ELFReader e, byte[] elfArray,ref ramSim ram){

		log.WriteLine ("RAM: Size {0}",ram.getSize());


		for (int prog = 0; prog < e.elfHeader.e_phnum; prog++) {
			int ramAddress = e.elfphs[prog].p_vaddr;
			log.WriteLine ("RAM: Writing to {0} ", ramAddress);

			int elfOffSet = (int)e.elfphs[prog].p_offset;
			log.WriteLine ("ELF: Reading from {0}", e.elfphs[prog].p_offset);

			log.WriteLine ("ELF: Size of Segment {0}", e.elfphs[prog].p_filesz);
			int ii = ramAddress;
			int j = elfOffSet;
			for (; j < elfArray.Length && ii < e.elfphs[prog].p_filesz + ramAddress; ii++, j++) {
				ram.WriteByte (ii, elfArray [j]);
			}


		}

	}

}



public class MD5{

	public string hashFile(string file){
		string text = File.ReadAllText (file);
		byte[] bA = new byte[text.Length * sizeof(char)];
		System.Buffer.BlockCopy(text.ToCharArray(), 0, bA, 0, bA.Length);

		string output = Hash (bA);
		return output;
	}
	public string Hash(byte[] theArray){
	
			
		// step 1, calculate MD5 hash from input
		System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();

		byte[] hash = md5.ComputeHash(theArray);

		// step 2, convert byte array to hex string
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < hash.Length; i++)
		{
			sb.Append(hash[i].ToString("X2"));
		}
		return sb.ToString();
	}

}

public class ramSim{
	//ReadWord / WriteWord / ReadHalfWord / WriteHalfWord / ReadByte / WriteByte
	private byte[] theArray;

	//program counter
	//Int32 pc;

	public ramSim(int memSize){
		theArray = new byte[memSize];

	}
	public string getHash(){
		MD5 hasher = new MD5 ();

		return hasher.Hash(theArray);
	}



	public byte[] getArray(){
		return theArray;
	}

	public int getSize(){
		return theArray.Length;
	}

	public bool TestFlag(Int32 addr,byte bit){
		if (bit >= 0 && bit < 32) {
			int word = ReadWord (addr);
			string binary = Convert.ToString (word, 2);
			binary = binary.PadLeft (32, '0');
			if (binary [bit] == '1') {
				return true;
			} else {
				return false;
			}
		}
		return false;

	}

	public void SetFlag(Int32 addr, byte bit, bool flag){
		if (bit >= 0 && bit < 32) {
			int word = ReadWord (addr);
			string binary = Convert.ToString (word, 2);
			binary = binary.PadLeft (32, '0');
			char[] binA = binary.ToCharArray();
			if (flag) {
				binA [bit] = '1';
			}else {
				binA [bit] = '0';
			}
			binary = new string (binA);
			word = Convert.ToInt32 (binary, 2);
			WriteWord (addr, word);
		}
	}

	public int ReadWord(Int32 addr){
		//if address is not divisible by 4 escape
		if (addr % 4 == 0) {
			int output = BitConverter.ToInt32(theArray, addr);
			return output;
		}
		return -1;
	}//ReadWord

	public short ReadHalfWord(Int32 addr){
		//if address is not divisible by 4 escape
		if (addr % 2 == 0) {
			short output = BitConverter.ToInt16(theArray, addr);
			return output;
		}
		return -1;
	}//ReadHalfWord
	
	public byte ReadByte(Int32 addr){
		byte output = theArray[addr];
		return output;
	}//ReadByte


	public void WriteWord(Int32 addr, int inpu){
		if (addr % 4 == 0) {
			byte[] intBytes = BitConverter.GetBytes (inpu);
			Array.Copy (intBytes, 0, theArray, addr, 4);
		}
	}//WriteWord
	
	public void WriteHalfWord(Int32 addr, short inpu){		
		if (addr % 2 == 0) {
			byte[] shortBytes = BitConverter.GetBytes (inpu);
			Array.Copy (shortBytes, 0, theArray, addr, 2);
		}
	}//WriteHalfWord
	
	public void WriteByte(Int32 addr, byte inpu){
		theArray [addr] = inpu;
	}//WriteByte

	public void CLEAR (){
		Array.Clear (theArray, 0, theArray.Length);
	}
}

//edit it!!!
// A struct that mimics memory layout of ELF program file header
// See http://www.sco.com/developers/gabi/latest/contents.html for details
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ELFPhdr {
	public int      p_type;
	public int      p_offset;
	public int      p_vaddr;
	public int      p_paddr;
	public int      p_filesz;
	public int      p_memsz;
	public int      p_flags;
	public int      p_align;
} //Elf32_Phdr;


// A struct that mimics memory layout of ELF file header
// See http://www.sco.com/developers/gabi/latest/contents.html for details
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ELF
{
	public byte EI_MAG0, EI_MAG1, EI_MAG2, EI_MAG3, EI_CLASS, EI_DATA, EI_VERSION;
	byte unused1, unused2, unused3, unused4, unused5, unused6, unused7, unused8, unused9;
	public ushort e_type;
	public ushort e_machine;
	public uint e_version;
	public uint e_entry;
	public uint e_phoff;
	public uint e_shoff;
	public uint e_flags;
	public ushort e_ehsize;
	public ushort e_phentsize;
	public ushort e_phnum;
	public ushort e_shentsize;
	public ushort e_shnum;
	public ushort e_shstrndx;
}

public class ELFReader{
	public ELF elfHeader;
	public ELFPhdr[] elfphs;

	//public ELF elfSection;


	public void ReadHeader(byte[] elfArray)
	{
	
	//	using (FileStream strm = new FileStream(elfFile, FileMode.Open))


		byte[] data = new byte[Marshal.SizeOf (elfHeader)];

		// Read ELF header into data
		Array.Copy (elfArray, data, data.Length);
		//strm.Read (data, 0, data.Length);
		// Convert to struct
		elfHeader = ByteArrayToStructure<ELF> (data);


		// seek to first program header entry
		int phEntry = (int)elfHeader.e_phoff;
		//strm.Seek (elfHeader.e_phoff, SeekOrigin.Begin);
		elfphs = new ELFPhdr[elfHeader.e_phnum];

		for (int i = 0; i < elfHeader.e_phnum; i++) {
			data = new byte[elfHeader.e_phentsize];
			Array.Copy (elfArray, phEntry, data, 0, elfHeader.e_phentsize);
			phEntry += elfHeader.e_phentsize;
			//strm.Read (data, 0, (int)elfHeader.e_phentsize);
			elfphs [i] = ByteArrayToStructure<ELFPhdr> (data);
		}//forloop

		// Now, do something with it ... see cppreadelf for a hint
		
	}

	// Converts a byte array to a struct
	static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
	{
		GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
		T stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
		                                    typeof(T));
		handle.Free();
		return stuff;
	}

}

public class TestArmSim{

	public static void RunTests(StreamWriter log){
		log.WriteLine ("Test: Starting ArmSim unit tests");

		ELFReader e = new ELFReader ();


		log.WriteLine ("Test: Testing Hash of test1.exe");
		byte[] elfArray = File.ReadAllBytes ("test1.exe");
		e.ReadHeader (elfArray);
		
		ramSim ram = new ramSim (32768);
		armsim.writeElfToRam (e, elfArray, ref ram);

        string resultHash = ram.getHash();
        string hash = "3500a8bef72dfed358b25b61b7602cf1";
		Debug.Assert( hash.ToUpper() == resultHash);

		ram.CLEAR ();

		log.WriteLine ("Test: Testing Hash of test2.exe");
		elfArray = File.ReadAllBytes ("test2.exe");
		e.ReadHeader (elfArray);
		armsim.writeElfToRam (e, elfArray, ref ram);
        resultHash = ram.getHash();
		hash = "0a81d8b63d44a192e5f9f52980f2792e";
		Debug.Assert( hash.ToUpper() == resultHash);

		ram.CLEAR ();

		log.WriteLine ("Test: Testing Hash of test3.exe");
		elfArray = File.ReadAllBytes ("test3.exe");
		e.ReadHeader (elfArray);
		armsim.writeElfToRam (e, elfArray, ref ram);
        resultHash = ram.getHash();
		hash = "977159b662ac4e450ed62063fba27029";
		Debug.Assert( hash.ToUpper() == resultHash);

		log.WriteLine ("Test: All Hashes correct\n");

	}

}

public class TestRam{

	public static void RunTests(StreamWriter log){
		log.WriteLine("Test: Starting RAM unit tests");
		ramSim tram = new ramSim (32768);
		
		log.WriteLine ("Test: Read/Write Byte");
		byte byteRes = tram.ReadByte (0);
		Debug.Assert (byteRes == 0);
		tram.WriteByte (0, 0xee);
		byteRes = tram.ReadByte (0);
		Debug.Assert (byteRes == 0xee);

		tram.CLEAR ();
		
		log.WriteLine("Test: Read/Write HalfWord");
		short shortRes = tram.ReadHalfWord (0);
		Debug.Assert (shortRes == 0);
		tram.WriteHalfWord (0, 0xeef);
		shortRes = tram.ReadHalfWord (0);
		Debug.Assert (shortRes == 0xeef);

		tram.CLEAR ();

		log.WriteLine("Test: Read/Write Word");
		int intRes = tram.ReadWord (0);
		Debug.Assert (intRes == 0);
		tram.WriteWord (0, 0xabcdef);
		intRes = tram.ReadWord (0);
		Debug.Assert (intRes == 0xabcdef);

		tram.CLEAR ();

		log.WriteLine ("Test: Set/Test Flag");
		bool flagRes = tram.TestFlag (0, 4);
		Debug.Assert (flagRes == false);
		tram.SetFlag (0, 4, true);
		flagRes = tram.TestFlag (0, 4);
		Debug.Assert (flagRes == true);
		flagRes = tram.TestFlag (0, 3);
		Debug.Assert (flagRes == false);


		log.WriteLine ("Test: All Ram Tests passed\n");
		tram.CLEAR ();

	}

}


public class Options{

	public string file;
	public int memSize;
	public bool test;
	public bool valid;


	//-----------------Getters
	public string getFile(){
		return file;
	}
	public int getMemSize(){
		return memSize;
	}
	public bool getTest(){
		return test;
	}

	public void getError(string inpu){
		string output = inpu;
		output += "\nValid input is: armsim --load elf-file [ --mem memory-size ] [ --test]";
		Console.WriteLine (output);
	}

	public bool parse(string[] inpu){
		file = null;
		test = false;
		valid = true;
		memSize = 32768;
		for (int i = 0; i < inpu.Length; i++) {
			switch (inpu[i])
			{
				case "--load":
				i++;
				file = inpu [i];
				break;

				case "--mem":
				i++;
				memSize = Convert.ToInt32 (inpu [i]);
				if (memSize > 1048576) {
					getError("Memsize is too large, cannot be over 1048576 bytes.");
					valid = false;
				}
				break;

				case "--test":
				test = true;
				break;

				default:
				//this can be the helper instructions
				getError(inpu [i] + " is an invalid option.");
				valid = false;
				break;
			}
		}
		if (file == null) {
			valid = false;
			getError("No file specified.");
		}
		return valid;
	}
}


