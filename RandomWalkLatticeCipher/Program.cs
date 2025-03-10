using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public class RandomWalkLatticeCipher
{
    private readonly int _dimensions;
    private readonly int _stepRange;
    private readonly RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();

    public RandomWalkLatticeCipher(int dimensions, int stepRange)
    {
        if (dimensions < 2) throw new ArgumentException("Dimensions must be at least 2.");
        _dimensions = dimensions;
        _stepRange = stepRange;
    }

    // Core random walk generator
    private int[] GenerateWalkStep(byte[] seed, int index, int previousStepSum, out byte[] newSeed)
    {
        using (var hmac = new HMACSHA256(seed))
        {
            byte[] hashInput = BitConverter.GetBytes(index)
                .Concat(BitConverter.GetBytes(previousStepSum))
                .ToArray();

            byte[] hash = new byte[_dimensions];
            for (int i = 0; i < _dimensions; i += 32)
            {
                byte[] chunk = hmac.ComputeHash(hashInput.Concat(BitConverter.GetBytes(i)).ToArray());
                Array.Copy(chunk, 0, hash, i, Math.Min(32, _dimensions - i));
            }

            int[] step = new int[_dimensions];
            for (int i = 0; i < _dimensions; i++)
            {
                step[i] = (hash[i] % (2 * _stepRange + 1)) - _stepRange; // [-stepRange, stepRange]
            }

            newSeed = hmac.ComputeHash(hash);
            return step;
        }
    }

    public string Encrypt(string message, string password)
    {
        byte[] seed = Encoding.UTF8.GetBytes(password);
        char[] ciphertext = new char[message.Length];
        int previousStepSum = 0;

        for (int i = 0; i < message.Length; i++)
        {
            int[] step = GenerateWalkStep(seed, i, previousStepSum, out seed);
            int stepSum = step.Sum();
            
            ciphertext[i] = (char)((message[i] + stepSum) ^ stepSum);

            previousStepSum = stepSum;
        }

        return new string(ciphertext);
    }

    public string Decrypt(string ciphertext, string password)
    {
        byte[] seed = Encoding.UTF8.GetBytes(password);
        char[] plaintext = new char[ciphertext.Length];
        int previousStepSum = 0;

        for (int i = 0; i < ciphertext.Length; i++)
        {
            int[] step = GenerateWalkStep(seed, i, previousStepSum, out seed);
            int stepSum = step.Sum();
            
            plaintext[i] = (char)((ciphertext[i] ^ stepSum) - stepSum);

            previousStepSum = stepSum;
        }

        return new string(plaintext);
    }
}

class Program
{
    static void Main()
    {
        var cipher = new RandomWalkLatticeCipher(dimensions: 100, stepRange: 50);
        string message = "HELLO WORLD, THIS IS A SECRET MESSAGE! !(*@&$%&";
        string password = "secret";

        string encrypted = cipher.Encrypt(message, password);
        Console.WriteLine($"Encrypted: {encrypted}");

        string decrypted = cipher.Decrypt(encrypted, password);
        Console.WriteLine($"Decrypted: {decrypted}");
    }
}
