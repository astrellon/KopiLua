using System;
using System.IO;

namespace KopiLua
{
	using lu_byte = System.Byte;
	using lu_int32 = System.Int32;
	using lu_mem = System.UInt32;
	using TValue = Lua.LuaTypeValue;
	using StkId = Lua.LuaTypeValue;
	using ptrdiff_t = System.Int32;
	using Instruction = System.UInt32;
	/*
		** `per thread' state
		*/
	public class LuaState : Lua.GCObject {

		public lu_byte status;
		public StkId top;  /* first free slot in the stack */
		public StkId base_;  /* base of current function */
		public Lua.GlobalState l_G;
		public Lua.CallInfo ci;  /* call info for current function */
		public InstructionPtr savedpc = new InstructionPtr();  /* `savedpc' of current function */
		public StkId stack_last;  /* last free slot in the stack */
		public StkId[] stack;  /* stack base */
		public Lua.CallInfo end_ci;  /* points after end of ci array*/
		public Lua.CallInfo[] base_ci;  /* array of CallInfo's */
		public int stacksize;
		public int size_ci;  /* size of array `base_ci' */
		[CLSCompliantAttribute(false)]
		public ushort nCcalls;  /* number of nested C calls */
		[CLSCompliantAttribute(false)]
		public ushort baseCcalls;  /* nested C calls when resuming coroutine */
		public lu_byte hookmask;
		public lu_byte allowhook;
		public int basehookcount;
		public int hookcount;
		public LuaHook hook;
		public TValue l_gt = new Lua.LuaTypeValue();  /* table of globals */
		public TValue env = new Lua.LuaTypeValue();  /* temporary place for environments */
		public Lua.GCObject openupval;  /* list of open upvalues in this stack */
		public Lua.GCObject gclist;
		public Lua.LuaLongJmp errorJmp;  /* current error recover point */
		public ptrdiff_t errfunc;  /* current error handling function (stack index) */

        // Modification
        public Stream StdOut;
        public Stream StdIn;
        public Stream StdErr;
        // Returns the environment value for a given key.
        public Func<string, string> GetEnvHandler;
        // Sets the environment value for a given key.
        public Action<string, string> SetEnvHandler;
        // Returns the ticks value for the current time. Int indicates some flags.
        public Func<int, long> GetTimeHandler;
        // Called when the os.exit command is called.
        public Action ExitHandler;
        // Calld when os.execute is called.
        public Func<string, int> ExecuteHandler;
        // Called when io.open is called
        public Func<string, FileMode, FileAccess, Stream> OpenFileHandler;
        // Called when os.remove is called
        public Func<string, int> RemoveFileHandler;
        // Called when os.rename is called
        public Func<string, string, int> RenameFileHandler;
        // Called when needing a new temporary filename, used by io.tmpfile and os.tmpname.
        public Func<string> GetTempFilenameHandler;
	}
}
