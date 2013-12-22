#if SL5 || WP

// From: http://scrypt.codeplex.com/
// License: http://scrypt.codeplex.com/license


using System;
using System.IO;

namespace ServiceStack
{
    /// <summary>
    /// Cryptographic Exception
    /// </summary>
    public class CryptographicException : System.Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Error Message</param>
        public CryptographicException(string message) : base(message) { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Error Message</param>
        /// <param name="innerException">Inner Exception</param>
        public CryptographicException(string message, Exception innerException) : base(message, innerException) { }
    }

    internal enum AlgorithmIdentifier : uint
    {
        /// <summary>
        /// CALG_RSA_KEYX = (ALG_CLASS_KEY_EXCHANGE|ALG_TYPE_RSA|ALG_SID_RSA_ANY)
        /// </summary>
        CALG_RSA_KEYX = (5 << 13) | (2 << 9) | 0,
    }

    internal enum KeyBlobType : byte
    {
        /// <summary>
        /// Simple Key BLOB
        /// </summary>
        SimpleBlob = 0x1,
        /// <summary>
        /// Public Key BLOB
        /// </summary>
        PublicKeyBlob = 0x6,
        /// <summary>
        /// Private Key BLOB
        /// </summary>
        PrivateKeyBlob = 0x7,
        /// <summary>
        /// PlainText Key BLOB
        /// </summary>
        PlainTextKeyBlob = 0x8,
        /// <summary>
        /// Opaque Key BLOB
        /// </summary>
        OpaqueKeyBlob = 0x9,
        /// <summary>
        /// Public Key BLOB Extended
        /// </summary>
        PublicKeyBlobEx = 0xA,
        /// <summary>
        /// Symmetric Wrap Key BLOB
        /// </summary>
        SymmetricWrapKeyBlob = 0xB
    }

    /// <summary>
    /// A BLOBHEADER / PUBLICKEYSTRUC structure (Import from WinCrypt.h)
    /// </summary>
    /// <note>http://msdn.microsoft.com/en-us/library/ms884652.aspx</note>
    internal struct BlobHeader
    {
        /// <summary>
        /// Key BLOB type. The only BLOB types currently defined are PUBLICKEYBLOB, PRIVATEKEYBLOB, and SIMPLEBLOB. Other key BLOB types will be defined as needed.
        /// </summary>
        public KeyBlobType BlobType;

        /// <summary>
        /// Version number of the key BLOB format. This member currently must always have a value of 0x02.
        /// </summary>
        public byte Version;

        /// <summary>
        /// Reserved for future use. This member must be set to zero.
        /// </summary>
        public ushort Reserved;

        /// <summary>
        /// Algorithm identifier for the key contained by the key BLOB structure
        /// </summary>
        public AlgorithmIdentifier KeyAlgorithm;

        #region FromBinary - Create and initialize structure from binary data

        /// <summary>
        /// Create and initialize structure from binary data
        /// </summary>
        /// <exception cref="CryptographicException">On validate errors</exception>
        /// <returns>Initialized structure</returns>
        public static BlobHeader FromBinary(BinaryReader reader)
        {
            var blobHeader = new BlobHeader
            {
                BlobType = (KeyBlobType)reader.ReadByte(),
                Version = reader.ReadByte(),
                Reserved = reader.ReadUInt16(),
                KeyAlgorithm = (AlgorithmIdentifier)reader.ReadUInt32()
            };

            // Validate
            if (blobHeader.BlobType != KeyBlobType.PublicKeyBlob && blobHeader.BlobType != KeyBlobType.PrivateKeyBlob)
                throw new CryptographicException(string.Format("Unsupported Key BLOB type [{0}] in BlobHeader",
                                                               blobHeader.BlobType));

            if (blobHeader.Version != 0x02)
                throw new CryptographicException(string.Format("Unknown version [{0}] in BlobHeader",
                                                               blobHeader.Version));

            if (blobHeader.KeyAlgorithm != AlgorithmIdentifier.CALG_RSA_KEYX)
                throw new CryptographicException(
                    string.Format("Unsupported algorithm identifier [{0:X4}] in BlobHeader",
                                  (uint)blobHeader.KeyAlgorithm));
            return blobHeader;
        }

        #endregion

        #region ToBinary - Serializes structure as binary data

        /// <summary>
        /// Serializes structure as binary data
        /// </summary>
        public void ToBinary(BinaryWriter writer)
        {
            writer.Write((byte)BlobType);
            writer.Write(Version);
            writer.Write(Reserved);
            writer.Write((uint)KeyAlgorithm);
        }

        #endregion

        #region FromRSAParameters - Create and initialize structure from RSAParameters

        /// <summary>
        /// Create and initialize structure from RSAParameters
        /// </summary>
        /// <returns>Initialized structure</returns>
        public static BlobHeader FromRSAParameters(KeyBlobType blobType)
        {
            var blobHeader = new BlobHeader
            {
                BlobType = blobType,
                Version = 0x02,
                Reserved = 0x0000,
                KeyAlgorithm = AlgorithmIdentifier.CALG_RSA_KEYX
            };

            return blobHeader;
        }

        #endregion

        #region ToString overriding

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("BlobType: {0} | Version: {1} | KeyAlgorithm: {2}", BlobType, Version, KeyAlgorithm);
        }

        #endregion
    }

    /// <summary>
    /// RSA public key data
    /// </summary>
    /// <note>http://msdn.microsoft.com/en-us/library/aa387685(v=VS.85).aspx</note>
    internal struct RSAPubKey
    {
        /// <summary>
        /// The magic member must be set to 0x31415352 (public only) / 0x32415352 (including private). This hex value is the ASCII encoding of RSA1 / RSA2.
        /// </summary>
        public uint Magic;

        /// <summary>
        /// # of bits in modulus
        /// </summary>
        public uint BitLength;

        /// <summary>
        /// Public exponent
        /// </summary>
        public uint PublicExponent;

        #region FromBinary - Create and initialize structure from binary data

        /// <summary>
        /// Create and initialize structure from binary data
        /// </summary>
        /// <exception cref="CryptographicException">On validate errors</exception>
        /// <returns>Initialized structure</returns>
        public static RSAPubKey FromBinary(BinaryReader reader)
        {
            var rsaPubKey = new RSAPubKey
            {
                Magic = reader.ReadUInt32(),
                BitLength = reader.ReadUInt32(),
                PublicExponent = reader.ReadUInt32()
            };

            // Validate
            if (rsaPubKey.Magic != 0x31415352 && rsaPubKey.Magic != 0x32415352)
                throw new CryptographicException(string.Format("Invalid magic number [0x{0:X4}] in RSAPubKey",
                                                               rsaPubKey.Magic));

            if (rsaPubKey.BitLength % 8 != 0)
                throw new CryptographicException(string.Format("Invalid # of bits in modulus [{0}] in RSAPubKey",
                                                               rsaPubKey.BitLength));

            return rsaPubKey;
        }

        #endregion

        #region ToBinary - Serializes structure as binary data

        /// <summary>
        /// Serializes structure as binary data
        /// </summary>
        public void ToBinary(BinaryWriter writer)
        {
            writer.Write(Magic);
            writer.Write(BitLength);
            writer.Write(PublicExponent);
        }

        #endregion

        #region FromRSAParameters - Create and initialize structure from RSAParameters

        /// <summary>
        /// Create and initialize structure from RSAParameters
        /// </summary>
        /// <returns>Initialized structure</returns>
        public static RSAPubKey FromRSAParameters(RSAParameters @params, bool includePrivateParameters)
        {
            var rsaPubKey = new RSAPubKey
            {
                Magic = (uint)(includePrivateParameters ? 0x32415352 : 0x31415352),
                BitLength = (uint)(@params.N.Length << 3),
            };

            var bytes = new byte[sizeof(uint)];
            bytes[sizeof(uint) - 1] = 0;

            for (int i = 0; i < @params.E.Length; i++)
            {
                bytes[i] = @params.E[@params.E.Length - i - 1];
            }

            rsaPubKey.PublicExponent = BitConverter.ToUInt32(bytes, 0);

            return rsaPubKey;
        }

        #endregion

        #region ToRSAParameters - Create and initialize RSAParameters structure

        /// <summary>
        /// Create and initialize RSAParameters structure
        /// </summary>
        /// <returns>Initialized structure</returns>
        /// <note>http://msdn.microsoft.com/en-us/library/system.security.cryptography.rsaparameters.aspx</note>
        public RSAParameters ToRSAParameters()
        {
            var bytes = BitConverter.GetBytes(PublicExponent);

            var @params = new RSAParameters
            {
                E = new byte[bytes[sizeof(uint) - 1] == 0 ? sizeof(uint) - 1 : sizeof(uint)]
            };

            for (int i = 0; i < @params.E.Length; i++)
            {
                @params.E[i] = bytes[@params.E.Length - i - 1];
            }

            return @params;
        }

        #endregion

        #region ToString overriding

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("BitLength: {0} | PublicExponent: {1:X4}", BitLength, PublicExponent);
        }

        #endregion
    }

    /// <summary>
    /// Private-key BLOBs, type PRIVATEKEYBLOB, are used to store private keys outside a CSP. Extended provider private-key BLOBs have the following format.
    /// </summary>
    internal struct PrivateKeyBlob
    {
        /// <summary>
        /// BLOB Header
        /// </summary>
        public BlobHeader Header;

        /// <summary>
        /// RSA public key data
        /// </summary>
        public RSAPubKey RSAPubKey;

        /// <summary>
        /// The modulus. This has a value of prime1 * prime2 and is often known as n.
        /// </summary>
        public byte[] Modulus;

        /// <summary>
        /// Prime number 1, often known as p.
        /// </summary>
        public byte[] Prime1;

        /// <summary>
        /// Prime number 2, often known as q.
        /// </summary>
        public byte[] Prime2;

        /// <summary>
        /// Exponent 1. This has a numeric value of d mod (p - 1).
        /// </summary>
        public byte[] Exponent1;

        /// <summary>
        /// Exponent 2. This has a numeric value of d mod (q - 1).
        /// </summary>
        public byte[] Exponent2;

        /// <summary>
        /// Coefficient. This has a numeric value of (inverse of q mod p).
        /// </summary>
        public byte[] Coefficient;

        /// <summary>
        /// Private exponent, often known as d.
        /// </summary>
        public byte[] PrivateExponent;

        #region FromBinary - Create and initialize structure from binary data

        /// <summary>
        /// Create and initialize structure from binary data
        /// </summary>
        /// <exception cref="CryptographicException">On validate errors</exception>
        /// <returns>Initialized structure</returns>
        public static PrivateKeyBlob FromBinary(BinaryReader reader)
        {
            var header = BlobHeader.FromBinary(reader);
            return FromBinary(reader, header);
        }

        /// <summary>
        /// Create and initialize structure from binary data with defined header
        /// </summary>
        /// <exception cref="CryptographicException">On validate errors</exception>
        /// <returns>Initialized structure</returns>
        public static PrivateKeyBlob FromBinary(BinaryReader reader, BlobHeader header)
        {
            var privateKeyBlob = new PrivateKeyBlob
            {
                Header = header,
                RSAPubKey = RSAPubKey.FromBinary(reader),
            };

            int byteLength = (int)(privateKeyBlob.RSAPubKey.BitLength >> 3);
            int wordLength = (int)(privateKeyBlob.RSAPubKey.BitLength >> 4);

            privateKeyBlob.Modulus = new byte[byteLength];
            reader.Read(privateKeyBlob.Modulus, 0, privateKeyBlob.Modulus.Length);

            privateKeyBlob.Prime1 = new byte[wordLength];
            reader.Read(privateKeyBlob.Prime1, 0, privateKeyBlob.Prime1.Length);

            privateKeyBlob.Prime2 = new byte[wordLength];
            reader.Read(privateKeyBlob.Prime2, 0, privateKeyBlob.Prime2.Length);

            privateKeyBlob.Exponent1 = new byte[wordLength];
            reader.Read(privateKeyBlob.Exponent1, 0, privateKeyBlob.Exponent1.Length);

            privateKeyBlob.Exponent2 = new byte[wordLength];
            reader.Read(privateKeyBlob.Exponent2, 0, privateKeyBlob.Exponent2.Length);

            privateKeyBlob.Coefficient = new byte[wordLength];
            reader.Read(privateKeyBlob.Coefficient, 0, privateKeyBlob.Coefficient.Length);

            privateKeyBlob.PrivateExponent = new byte[byteLength];
            reader.Read(privateKeyBlob.PrivateExponent, 0, privateKeyBlob.PrivateExponent.Length);

            return privateKeyBlob;
        }

        #endregion

        #region ToBinary - Serializes structure as binary data

        /// <summary>
        /// Serializes structure as binary data
        /// </summary>
        public void ToBinary(BinaryWriter writer)
        {
            Header.ToBinary(writer);
            RSAPubKey.ToBinary(writer);

            writer.Write(Modulus);
            writer.Write(Prime1);
            writer.Write(Prime2);
            writer.Write(Exponent1);
            writer.Write(Exponent2);
            writer.Write(Coefficient);
            writer.Write(PrivateExponent);
        }

        #endregion

        #region FromRSAParameters - Create and initialize structure from RSAParameters

        /// <summary>
        /// Create and initialize structure from RSAParameters
        /// </summary>
        /// <returns>Initialized structure</returns>
        /// <note>http://msdn.microsoft.com/en-us/library/system.security.cryptography.rsaparameters.aspx</note>
        public static PrivateKeyBlob FromRSAParameters(RSAParameters @params)
        {
            var privateKeyBlob = new PrivateKeyBlob
            {
                Header = BlobHeader.FromRSAParameters(KeyBlobType.PrivateKeyBlob),
                RSAPubKey = RSAPubKey.FromRSAParameters(@params, true),
            };

            privateKeyBlob.Modulus = new byte[@params.N.Length];
            for (int i = 0; i < privateKeyBlob.Modulus.Length; i++)
            {
                privateKeyBlob.Modulus[i] = @params.N[@params.N.Length - i - 1];
            }

            privateKeyBlob.Prime1 = new byte[@params.P.Length];
            for (int i = 0; i < privateKeyBlob.Prime1.Length; i++)
            {
                privateKeyBlob.Prime1[i] = @params.P[@params.P.Length - i - 1];
            }

            privateKeyBlob.Prime2 = new byte[@params.Q.Length];
            for (int i = 0; i < privateKeyBlob.Prime2.Length; i++)
            {
                privateKeyBlob.Prime2[i] = @params.Q[@params.Q.Length - i - 1];
            }

            privateKeyBlob.Exponent1 = new byte[@params.DP.Length];
            for (int i = 0; i < privateKeyBlob.Exponent1.Length; i++)
            {
                privateKeyBlob.Exponent1[i] = @params.DP[@params.DP.Length - i - 1];
            }

            privateKeyBlob.Exponent2 = new byte[@params.DQ.Length];
            for (int i = 0; i < privateKeyBlob.Exponent2.Length; i++)
            {
                privateKeyBlob.Exponent2[i] = @params.DQ[@params.DQ.Length - i - 1];
            }

            privateKeyBlob.Coefficient = new byte[@params.IQ.Length];
            for (int i = 0; i < privateKeyBlob.Coefficient.Length; i++)
            {
                privateKeyBlob.Coefficient[i] = @params.IQ[@params.IQ.Length - i - 1];
            }

            privateKeyBlob.PrivateExponent = new byte[@params.D.Length];
            for (int i = 0; i < privateKeyBlob.PrivateExponent.Length; i++)
            {
                privateKeyBlob.PrivateExponent[i] = @params.D[@params.D.Length - i - 1];
            }

            return privateKeyBlob;
        }

        #endregion

        #region ToRSAParameters - Create and initialize RSAParameters structure

        /// <summary>
        /// Create and initialize RSAParameters structure
        /// </summary>
        /// <returns>Initialized structure</returns>
        /// <note>http://msdn.microsoft.com/en-us/library/system.security.cryptography.rsaparameters.aspx</note>
        public RSAParameters ToRSAParameters()
        {
            var @params = RSAPubKey.ToRSAParameters();

            @params.N = new byte[Modulus.Length];
            for (int i = 0; i < @params.N.Length; i++)
            {
                @params.N[i] = Modulus[Modulus.Length - i - 1];
            }

            @params.P = new byte[Prime1.Length];
            for (int i = 0; i < @params.P.Length; i++)
            {
                @params.P[i] = Prime1[Prime1.Length - i - 1];
            }

            @params.Q = new byte[Prime2.Length];
            for (int i = 0; i < @params.Q.Length; i++)
            {
                @params.Q[i] = Prime2[Prime2.Length - i - 1];
            }

            @params.DP = new byte[Exponent1.Length];
            for (int i = 0; i < @params.DP.Length; i++)
            {
                @params.DP[i] = Exponent1[Exponent1.Length - i - 1];
            }

            @params.DQ = new byte[Exponent2.Length];
            for (int i = 0; i < @params.DQ.Length; i++)
            {
                @params.DQ[i] = Exponent2[Exponent2.Length - i - 1];
            }

            @params.IQ = new byte[Coefficient.Length];
            for (int i = 0; i < @params.IQ.Length; i++)
            {
                @params.IQ[i] = Coefficient[Coefficient.Length - i - 1];
            }

            @params.D = new byte[PrivateExponent.Length];
            for (int i = 0; i < @params.D.Length; i++)
            {
                @params.D[i] = PrivateExponent[PrivateExponent.Length - i - 1];
            }

            return @params;
        }

        #endregion

        #region ToString overriding

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Header:\r\n{0}\r\nRSAPubKey:\r\n{1}\r\nModulus: {2}", Header, RSAPubKey,
                                 BitConverter.ToString(Modulus));
        }

        #endregion

    }

    /// <summary>
    /// Public-key BLOBs, type PUBLICKEYBLOB, are used to store public keys outside a CSP. Extended provider public-key BLOBs have the following format.
    /// </summary>
    internal struct PublicKeyBlob
    {
        /// <summary>
        /// BLOB Header
        /// </summary>
        public BlobHeader Header;

        /// <summary>
        /// RSA public key data
        /// </summary>
        public RSAPubKey RSAPubKey;

        /// <summary>
        /// The public-key modulus data
        /// </summary>
        public byte[] Modulus;

        #region FromBinary - Create and initialize structure from binary data

        /// <summary>
        /// Create and initialize structure from binary data
        /// </summary>
        /// <exception cref="CryptographicException">On validate errors</exception>
        /// <returns>Initialized structure</returns>
        public static PublicKeyBlob FromBinary(BinaryReader reader)
        {
            BlobHeader header = BlobHeader.FromBinary(reader);

            return FromBinary(reader, header);
        }

        /// <summary>
        /// Create and initialize structure from binary data with defined header
        /// </summary>
        /// <exception cref="CryptographicException">On validate errors</exception>
        /// <returns>Initialized structure</returns>
        public static PublicKeyBlob FromBinary(BinaryReader reader, BlobHeader header)
        {
            var publicKeyBlob = new PublicKeyBlob
            {
                Header = header,
                RSAPubKey = RSAPubKey.FromBinary(reader),
            };

            int modulusLength = (int)(publicKeyBlob.RSAPubKey.BitLength >> 3);

            publicKeyBlob.Modulus = new byte[modulusLength];
            reader.Read(publicKeyBlob.Modulus, 0, publicKeyBlob.Modulus.Length);

            return publicKeyBlob;
        }

        #endregion

        #region ToBinary - Serializes structure as binary data

        /// <summary>
        /// Serializes structure as binary data
        /// </summary>
        public void ToBinary(BinaryWriter writer)
        {
            Header.ToBinary(writer);
            RSAPubKey.ToBinary(writer);
            writer.Write(Modulus);
        }

        #endregion

        #region FromRSAParameters - Create and initialize structure from RSAParameters

        /// <summary>
        /// Create and initialize structure from RSAParameters
        /// </summary>
        /// <returns>Initialized structure</returns>
        /// <note>http://msdn.microsoft.com/en-us/library/system.security.cryptography.rsaparameters.aspx</note>
        public static PublicKeyBlob FromRSAParameters(RSAParameters @params)
        {
            var publicKeyBlob = new PublicKeyBlob
            {
                Header = BlobHeader.FromRSAParameters(KeyBlobType.PublicKeyBlob),
                RSAPubKey = RSAPubKey.FromRSAParameters(@params, false),

                Modulus = new byte[@params.N.Length],
            };

            for (int i = 0; i < publicKeyBlob.Modulus.Length; i++)
            {
                publicKeyBlob.Modulus[i] = @params.N[@params.N.Length - i - 1];
            }

            return publicKeyBlob;
        }

        #endregion

        #region ToRSAParameters - Create and initialize RSAParameters structure

        /// <summary>
        /// Create and initialize RSAParameters structure
        /// </summary>
        /// <returns>Initialized structure</returns>
        /// <note>http://msdn.microsoft.com/en-us/library/system.security.cryptography.rsaparameters.aspx</note>
        public RSAParameters ToRSAParameters()
        {
            var @params = RSAPubKey.ToRSAParameters();

            @params.N = new byte[Modulus.Length];

            for (int i = 0; i < @params.N.Length; i++)
            {
                @params.N[i] = Modulus[Modulus.Length - i - 1];
            }

            return @params;
        }

        #endregion

        #region ToString overriding

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Header:\r\n{0}\r\nRSAPubKey:\r\n{1}\r\nModulus: {2}", Header, RSAPubKey,
                                 BitConverter.ToString(Modulus));
        }

        #endregion
    }

    /// <summary>
    /// RSAParameters for Import / Export
    /// </summary>
    public class RSAParameters
    {
        private byte[] m_E = new byte[] { };
        private byte[] m_N = new byte[] { };
        private byte[] m_P = new byte[] { };
        private byte[] m_Q = new byte[] { };
        private byte[] m_DP = new byte[] { };
        private byte[] m_DQ = new byte[] { };
        private byte[] m_IQ = new byte[] { };
        private byte[] m_D = new byte[] { };
        private byte[] m_Phi = new byte[] { };

        /// <summary>
        /// Parameter value E
        /// </summary>
        public byte[] E
        {
            get { return m_E; }
            set { m_E = value; }
        }

        /// <summary>
        /// Parameter value N
        /// </summary>
        public byte[] N
        {
            get { return m_N; }
            set { m_N = value; }
        }

        /// <summary>
        /// Parameter value P
        /// </summary>
        public byte[] P
        {
            get { return m_P; }
            set { m_P = value; }
        }

        /// <summary>
        /// Parameter value Q
        /// </summary>
        public byte[] Q
        {
            get { return m_Q; }
            set { m_Q = value; }
        }

        /// <summary>
        /// Parameter value DP
        /// </summary>
        public byte[] DP
        {
            get { return m_DP; }
            set { m_DP = value; }
        }

        /// <summary>
        /// Parameter value DQ
        /// </summary>
        public byte[] DQ
        {
            get { return m_DQ; }
            set { m_DQ = value; }
        }

        /// <summary>
        /// Parameter value IQ
        /// </summary>
        public byte[] IQ
        {
            get { return m_IQ; }
            set { m_IQ = value; }
        }

        /// <summary>
        /// Parameter value D
        /// </summary>
        public byte[] D
        {
            get { return m_D; }
            set { m_D = value; }
        }

        /// <summary>
        /// Parameter value Phi
        /// </summary>
        internal byte[] Phi
        {
            get { return m_Phi; }
            set { m_Phi = value; }
        }
    }
}

#endif