using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

public static class BinaryString
{
    /// <summary>
    /// 将字节数组转换为表示它的二进制字符串
    /// </summary>
    public static string FromBytes(byte[] buffer)
    {
        System.Span<char> chars = stackalloc char[buffer.Length * 8];//StringBuilder也可以, 但这样性能更高
        int bitIdx = -1;
        for (int byteIdx = 0; byteIdx < buffer.Length; byteIdx++)
            for (int bitByteIdx = 7; bitByteIdx >= 0; bitByteIdx--)
                chars[++bitIdx] = (char)((buffer[byteIdx] >> bitByteIdx & 1) + '0');
        return new(chars);
    }

    /// <summary>
    /// 把任意值类型转换为二进制字符串
    /// </summary>
    /// <typeparam name="T">要转换的类型.</typeparam>
    /// <param name="x">要转换的值</param>
    /// <param name="length">从最低位起, 要的bit数量. 如果不填, 则会根据有效位自动确定. 若小于有效位数, 则会截断.</param>
    /// <returns>转换后的字符串.</returns>
    /// <exception cref="ArgumentOutOfRangeException">如果length大于T类型的bit数量, 为防止内存越界读取, 会丢这个东西</exception>
    public static unsafe string FromValue<T>(T x, int length = 0) where T : unmanaged
    {
        Trace.Assert(length <= sizeof(T) * 8, $"length {length} 不能大于{typeof(T)}的位长度!");

        StringBuilder sb = new();
        byte* px = (byte*)&x;

        if (length <= 0)
        {
            length = sizeof(T) * 8;
            while (BitAt(px, length - 1) == 0 && --length > 0) ;
        }

        while (--length >= 0)
            sb.Append((char)(BitAt(px, length) + '0'));

        return sb.ToString();

        static int BitAt(byte* p, int bitIdx)
        {
            int byteIdx = System.Math.DivRem(bitIdx, 8, out int rem);
            return (p[byteIdx] >> rem) & 1;
        }
    }

    /// <summary>
    /// 将字符串表示的二进制其转换为对应的字节数组
    /// </summary>
    /// <param name="binStr">只含有0/1的字符串</param>
    /// <returns>如果输入的字符串含有0/1以外的字符, 结果未定义</returns>
    public static byte[] Parse(string binStr)
    {
        int byteCount = binStr.Length >> 3;
        byte[] buffer = new byte[byteCount];
        for (int byteIdx = 0; byteIdx < byteCount; byteIdx++)
            for (int bitIdx = 0; bitIdx < 8; bitIdx++)
            {
                int bit0or1 = (binStr[byteIdx * 8 + bitIdx] - '0');
                buffer[byteIdx] |= (byte)(bit0or1 << (7 - bitIdx));
            }
        return buffer;
    }

    /// <summary>
    /// 将字符串表示的二进制其转换为对应的字节数组
    /// </summary>
    /// <param name="binStr">只含有0或1的字符串<</param>
    /// <param name="buffer">输出的字节数组</param>
    /// <returns> 若字符串只含有字符0或1则返回0, 否则返回第一个非0或1字符 </returns>
    public static char TryParse(string binStr, out byte[] buffer)
    {
        int byteCount = binStr.Length >> 3;
        buffer = new byte[byteCount];
        for (int byteIdx = 0; byteIdx < byteCount; byteIdx++)
            for (int bitIdx = 0; bitIdx <= 7; bitIdx++)
                if (TryParseBinChar(binStr[byteIdx * 8 + bitIdx], out int bit0or1))
                    buffer[byteIdx] |= (byte)(bit0or1 << (7 - bitIdx));
                else
                    return binStr[byteIdx * 8 + bitIdx];
        return '\0';
    }

    /// <summary>
    /// 尝试把二进制字符串转换为二进制变量.
    /// </summary>
    /// <param name="result">如果转换失败, 返回default(Tbin)</param>
    /// <returns>如果转换成功则返回'\0', 如果有'0'或'1'以外的字符则返回对应的字符值</returns>
    public static char TryParse<Tbin>(string binStr, out Tbin result) where Tbin : unmanaged
    {
        Debug.Assert(Unsafe.SizeOf<Tbin>() <= 8, "暂不支持转换为大于64位的类型");
        result = default;
        long tmp = 0;
        for (int i = 0; i < binStr.Length; i++)
        {
            tmp <<= 1;
            if (TryParseBinChar(binStr[i], out int val))
            {
                tmp |= (long)val;
            }
            else return binStr[i];
        }
        result = Unsafe.As<long, Tbin>(ref tmp);
        return '\0';
    }

    public static bool TryParseBinChar(char c, out int result)
    {
        result = c - '0';
        return result == (result & 1);
    }
}