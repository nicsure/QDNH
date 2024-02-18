using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace QDNH.Network
{
    public class Authenticator
    {
        public byte[] Salt { get; private set; }
        public byte[] Hash { get; private set; }
        public Authenticator(string password) 
        {
            Salt = RandomNumberGenerator.GetBytes(32);
            byte[] b = Encoding.ASCII.GetBytes(password).Concat(Salt).ToArray();
            using SHA256 sha = SHA256.Create();
            Hash = sha.ComputeHash(b);
        }

        public bool Challenge(byte[] challenge) 
        {
            return challenge.SequenceEqual(Hash);
        }
    }
}
