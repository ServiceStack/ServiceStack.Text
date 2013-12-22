#if SL5 || WP

// From: http://scrypt.codeplex.com/
// License: http://scrypt.codeplex.com/license


using System;

namespace ServiceStack
{
    /// <summary>
    /// Interface which must be implemented by all custom padding providers. 
    /// Padding is used to provide randomness and unpredictability to the data 
    /// before it is encrypted.
    /// </summary>
    public interface IPaddingProvider
    {

        /// <summary>
        /// Adds padding to the input data and returns the padded data.
        /// </summary>
        /// <param name="dataBytes">Data to be padded prior to encryption</param>
        /// <param name="params">RSA Parameters used for padding computation</param>
        /// <returns>Padded message</returns>
        byte[] EncodeMessage(byte[] dataBytes, RSAParameters @params);
        /// <summary>
        /// Removes padding that was added to the unencrypted data prior to encryption.
        /// </summary>
        /// <param name="dataBytes">Data to have padding removed</param>
        /// <param name="params">RSA Parameters used for padding computation</param>
        /// <returns>Unpadded message</returns>
        byte[] DecodeMessage(byte[] dataBytes, RSAParameters @params);
        /// <summary>
        /// Gets the maximum message length for this padding provider.
        /// </summary>
        /// <param name="params">RSA Parameters used for padding computation</param>
        /// <returns>Max message length</returns>
        int GetMaxMessageLength(RSAParameters @params);
    }

    /// <summary>
    /// Uses PKCS#1 v 1.5 padding scheme to pad the data.
    /// </summary>
    /// <remarks></remarks>
    public sealed class PKCS1v1_5 : IPaddingProvider
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public PKCS1v1_5() { }


        /// <summary>
        /// Adds padding to the input data and returns the padded data.
        /// </summary>
        /// <param name="dataBytes">Data to be padded prior to encryption</param>
        /// <param name="params">RSA Parameters used for padding computation</param>
        /// <returns>Padded message</returns>
        public byte[] EncodeMessage(byte[] dataBytes, RSAParameters @params)
        {
            //Determine if we can add padding.
            if (dataBytes.Length > GetMaxMessageLength(@params))
            {
                throw new CryptographicException("Data length is too long.  Increase your key size or consider encrypting less data.");
            }

            int padLength = @params.N.Length - dataBytes.Length - 3;
            BigInteger biRnd = new BigInteger();
            biRnd.genRandomBits(padLength * 8, new Random(DateTime.Now.Millisecond));

            byte[] bytRandom = null;
            bytRandom = biRnd.getBytes();

            int z1 = bytRandom.Length;

            //Make sure the bytes are all > 0.
            for (int i = 0; i <= bytRandom.Length - 1; i++)
            {
                if (bytRandom[i] == 0x00)
                {
                    bytRandom[i] = 0x01;
                }
            }

            byte[] result = new byte[@params.N.Length];


            //Add the starting 0x00 byte
            result[0] = 0x00;

            //Add the version code 0x02 byte
            result[1] = 0x02;

            for (int i = 0; i <= bytRandom.Length - 1; i++)
            {
                z1 = i + 2;
                result[z1] = bytRandom[i];
            }

            //Add the trailing 0 byte after the padding.
            result[bytRandom.Length + 2] = 0x00;

            //This starting index for the unpadded data.
            int idx = bytRandom.Length + 3;

            //Copy the unpadded data to the padded byte array.
            dataBytes.CopyTo(result, idx);

            return result;
        }

        /// <summary>
        /// Removes padding that was added to the unencrypted data prior to encryption.
        /// </summary>
        /// <param name="dataBytes">Data to have padding removed</param>
        /// <param name="params">RSA Parameters used for padding computation</param>
        /// <returns>Unpadded message</returns>
        public byte[] DecodeMessage(byte[] dataBytes, RSAParameters @params)
        {
            byte[] bytDec = new byte[@params.N.Length];

            int lenDiff = 0;

            dataBytes.CopyTo(bytDec, lenDiff);

            if ((bytDec[0] != 0x0) && (bytDec[1] != 0x02))
            {
                throw new CryptographicException("Invalid padding.  Supplied data does not contain valid PKCS#1 v1.5 padding.  Padding could not be removed.");
            }

            //Find out where the padding ends.
            int idxEnd = 0;
            int dataLength = 0;

            for (int i = 2; i < bytDec.Length; i++)
            {
                if (bytDec[i] == 0x00)
                {
                    idxEnd = i;
                    break;
                }
            }

            //Calculate the length of the unpadded data
            dataLength = bytDec.Length - idxEnd - 2;

            byte[] result = new byte[dataLength + 1];

            int idxRslt = 0;

            //Put the unpadded data into the result array
            for (int i = idxEnd + 1; i <= bytDec.Length - 1; i++)
            {
                result[idxRslt] = bytDec[i];
                idxRslt += 1;
            }

            return result;
        }

        /// <summary>
        /// Gets the maximum message length for this padding provider.
        /// </summary>
        /// <param name="params">RSA Parameters used for padding computation</param>
        /// <returns>Max message length</returns>
        public int GetMaxMessageLength(RSAParameters @params)
        {
            return @params.N.Length - 11;
        }

    }


    /// <summary>
    /// The NoPadding class does not add any padding to the data.  
    /// This is not recommended.
    /// </summary>
    public sealed class NoPadding : IPaddingProvider
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public NoPadding() { }

        /// <summary>
        /// Adds padding to the input data and returns the padded data.
        /// </summary>
        /// <param name="dataBytes">Data to be padded prior to encryption</param>
        /// <param name="params">RSA Parameters used for padding computation</param>
        /// <returns>Padded message</returns>
        byte[] IPaddingProvider.EncodeMessage(byte[] dataBytes, RSAParameters @params)
        {
            return dataBytes;
        }
        /// <summary>
        /// Removes padding that was added to the unencrypted data prior to encryption.
        /// </summary>
        /// <param name="dataBytes">Data to have padding removed</param>
        /// <param name="params">RSA Parameters used for padding computation</param>
        /// <returns>Unpadded message</returns>
        byte[] IPaddingProvider.DecodeMessage(byte[] dataBytes, RSAParameters @params)
        {
            return dataBytes;
        }

        /// <summary>
        /// Gets the maximum message length for this padding provider.
        /// </summary>
        /// <param name="params">RSA Parameters used for padding computation</param>
        /// <returns>Max message length</returns>
        public int GetMaxMessageLength(RSAParameters @params)
        {
            return int.MaxValue;
        }
    }


    /// <summary>
    /// Uses OAEP Padding defined in PKCS#1 v 2.1.  Uses the 
    /// default standard SHA1 hash.  This padding provider is 
    /// compatible with .NET's OAEP implementation.
    /// </summary>
    public sealed class OAEP : IPaddingProvider
    {
        private IHashProvider m_hashProvider;
        //Hash length.  For SHA1, the length is 20 bytes.
        private int m_hLen = 20;
        //Length of message (dataBytes)
        private int m_mLen;
        //Number of bytes in the public key modulus
        private int m_k;

        /// <summary>
        /// Default constructor.  Uses the default SHA1 Hash for OAEP hash calculation.
        /// </summary>
        public OAEP()
        {
            m_hashProvider = new SHA1();
        }

        /// <summary>
        /// Internal constructor (used to perform OAEP with a different hash and hash output length
        /// </summary>
        /// <param name="ohashProvider"></param>
        /// <param name="hashLength"></param>
        internal OAEP(IHashProvider ohashProvider, int hashLength)
        {
            m_hashProvider = ohashProvider;
            m_hLen = hashLength;
        }

        /// <summary>
        /// Adds padding to the input data and returns the padded data.
        /// </summary>
        /// <param name="dataBytes">Data to be padded prior to encryption</param>
        /// <param name="params">RSA Parameters used for padding computation</param>
        /// <returns>Padded message</returns>
        public byte[] EncodeMessage(byte[] dataBytes, RSAParameters @params)
        {
            //Iterator
            int i = 0;

            //Get the size of the data to be encrypted
            m_mLen = dataBytes.Length;

            //Get the size of the public modulus (will serve as max length for cipher text)
            m_k = @params.N.Length;

            if (m_mLen > GetMaxMessageLength(@params))
            {
                throw new CryptographicException("Bad Data.");
            }

            //Generate the random octet seed (same length as hash)
            BigInteger biSeed = new BigInteger();
            biSeed.genRandomBits(m_hLen * 8, new Random());
            byte[] bytSeed = biSeed.getBytesRaw();

            //Make sure all of the bytes are greater than 0.
            for (i = 0; i <= bytSeed.Length - 1; i++)
            {
                if (bytSeed[i] == 0x00)
                {
                    //Replacing with the prime byte 17, no real reason...just picked at random.
                    bytSeed[i] = 0x17;
                }
            }

            //Mask the seed with MFG Function(SHA1 Hash)
            //This is the mask to be XOR'd with the DataBlock below.
            byte[] dbMask = Mathematics.OAEPMGF(bytSeed, m_k - m_hLen - 1, m_hLen, m_hashProvider);

            //Compute the length needed for PS (zero padding) and 
            //fill a byte array to the computed length
            int psLen = GetMaxMessageLength(@params) - m_mLen;

            //Generate the SHA1 hash of an empty L (Label).  Label is not used for this 
            //application of padding in the RSA specification.
            byte[] lHash = m_hashProvider.ComputeHash(System.Text.Encoding.UTF8.GetBytes(string.Empty.ToCharArray()));

            //Create a dataBlock which will later be masked.  The 
            //data block includes the concatenated hash(L), PS, 
            //a 0x01 byte, and the message.
            int dbLen = m_hLen + psLen + 1 + m_mLen;
            byte[] dataBlock = new byte[dbLen];

            int cPos = 0;
            //Current position

            //Add the L Hash to the data blcok
            for (i = 0; i <= lHash.Length - 1; i++)
            {
                dataBlock[cPos] = lHash[i];
                cPos += 1;
            }

            //Add the zero padding
            for (i = 0; i <= psLen - 1; i++)
            {
                dataBlock[cPos] = 0x00;
                cPos += 1;
            }

            //Add the 0x01 byte
            dataBlock[cPos] = 0x01;
            cPos += 1;

            //Add the message
            for (i = 0; i <= dataBytes.Length - 1; i++)
            {
                dataBlock[cPos] = dataBytes[i];
                cPos += 1;
            }

            //Create the masked data block.
            byte[] maskedDB = Mathematics.BitwiseXOR(dbMask, dataBlock);

            //Create the seed mask
            byte[] seedMask = Mathematics.OAEPMGF(maskedDB, m_hLen, m_hLen, m_hashProvider);

            //Create the masked seed
            byte[] maskedSeed = Mathematics.BitwiseXOR(bytSeed, seedMask);

            //Create the resulting cipher - starting with a 0 byte.
            byte[] result = new byte[@params.N.Length];
            result[0] = 0x00;

            //Add the masked seed
            maskedSeed.CopyTo(result, 1);

            //Add the masked data block
            maskedDB.CopyTo(result, maskedSeed.Length + 1);

            return result;
        }

        /// <summary>
        /// Removes padding that was added to the unencrypted data prior to encryption.
        /// </summary>
        /// <param name="dataBytes">Data to have padding removed</param>
        /// <param name="params">RSA Parameters used for padding computation</param>
        /// <returns>Unpadded message</returns>
        public byte[] DecodeMessage(byte[] dataBytes, RSAParameters @params)
        {
            m_k = @params.D.Length;
            if (!(m_k == dataBytes.Length))
            {
                throw new CryptographicException("Bad Data.");
            }

            //Length of the datablock
            int iDBLen = dataBytes.Length - m_hLen - 1;

            //Starting index for the data block.  This will be equal to m_hLen + 1.  The 
            //index is zero based, and the dataBytes should start with a single leading byte, 
            //plus the maskedSeed (equal to hash length m_hLen).
            int iDBidx = m_hLen + 1;

            //Single byte for leading byte
            byte bytY = 0;

            //Byte array matching the length of the hashing algorithm.
            //This array will hold the masked seed.
            byte[] maskedSeed = new byte[m_hLen];

            //Byte array matching the length of the following:
            //Private Exponent D minus Hash Length, minus 1 (for the leading byte)
            byte[] maskedDB = new byte[iDBLen];

            //Copy the leading byte
            bytY = dataBytes[0];

            //Copy the mask
            Array.Copy(dataBytes, 1, maskedSeed, 0, m_hLen);

            //Copy the data block
            Array.Copy(dataBytes, iDBidx, maskedDB, 0, iDBLen);

            //Reproduce the seed mask from the masked data block using the mask generation function
            byte[] seedMask = Mathematics.OAEPMGF(maskedDB, m_hLen, m_hLen, m_hashProvider);

            //Reproduce the Seed from the Seed Mask.
            byte[] seed = Mathematics.BitwiseXOR(maskedSeed, seedMask);

            //Reproduce the data block bask from the seed using the mask generation function
            byte[] dbMask = Mathematics.OAEPMGF(seed, m_k - m_hLen - 1, m_hLen, m_hashProvider);

            //Reproduce the data block from the masked data block and the seed
            byte[] dataBlock = Mathematics.BitwiseXOR(maskedDB, dbMask);

            //Pull the message from the data block.  First m_hLen bytes are the lHash, 
            //followed by padding of 0x00's, followed by a single 0x01, then the message.
            //So we're going to start and index m_hLen and work forward.
            if (!(dataBlock[m_hLen] == 0x00))
            {
                throw new CryptographicException("Decryption Error.  Bad Data.");
            }

            //If we passed the 0x00 first byte test, iterate through the 
            //data block and find the terminating character.
            int iDataIdx = 0;


            for (int i = m_hLen; i <= dataBlock.Length - 1; i++)
            {
                if (dataBlock[i] == 0x01)
                {
                    iDataIdx = i + 1;
                    break;
                }
            }

            //Now find the length of the data and copy it to a byte array.
            int iDataLen = dataBlock.Length - iDataIdx;
            byte[] result = new byte[iDataLen];
            Array.Copy(dataBlock, iDataIdx, result, 0, iDataLen);

            return result;
        }

        /// <summary>
        /// Gets the maximum message length for this padding provider.
        /// </summary>
        /// <param name="params">RSA Parameters used for padding computation</param>
        /// <returns>Max message length</returns>
        public int GetMaxMessageLength(RSAParameters @params)
        {
            return @params.N.Length - (2 * m_hLen) - 2;
        }

    }

    /// <summary>
    /// Uses OAEP Padding Scheme defined in PKCS#1 v 2.1.  Uses a 
    /// SHA256 hash.  This padding provider is currently 
    /// not compatible with .NET's OAEP implementation.
    /// </summary>
    public sealed class OAEP256 : IPaddingProvider
    {

        //To avoid duplicating code, we're using an  
        //OAEP padding provider that has an internal accessible constructor 
        //that allows us to specify the hashprovider and hash length.  
        //This will also allow us to easily add new hash providers to 
        //the current OAEP implementation.

        private OAEP m_OAEP;
        /// <summary>
        /// Default constructor.  Uses a SHA256 Hash for OAEP hash calculation.
        /// This PaddingProvider provides added security to message padding, 
        /// however it requires the data to be encrypted to be shorter and 
        /// is not compatible with the RSACryptoServiceProvider's implementation 
        /// of OAEP.
        /// </summary>
        public OAEP256()
        {
            m_OAEP = new OAEP(new SHA256(), 32);
        }

        /// <summary>
        /// Adds padding to the input data and returns the padded data.
        /// </summary>
        /// <param name="dataBytes">Data to be padded prior to encryption</param>
        /// <param name="params">RSA Parameters used for padding computation</param>
        /// <returns>Padded message</returns>
        public byte[] EncodeMessage(byte[] dataBytes, RSAParameters @params)
        {
            return m_OAEP.EncodeMessage(dataBytes, @params);
        }

        /// <summary>
        /// Removes padding that was added to the unencrypted data prior to encryption.
        /// </summary>
        /// <param name="dataBytes">Data to have padding removed</param>
        /// <param name="params">RSA Parameters used for padding computation</param>
        /// <returns>Unpadded message</returns>
        public byte[] DecodeMessage(byte[] dataBytes, RSAParameters @params)
        {
            return m_OAEP.DecodeMessage(dataBytes, @params);
        }

        /// <summary>
        /// Gets the maximum message length for this padding provider.
        /// </summary>
        /// <param name="params">RSA Parameters used for padding computation</param>
        /// <returns>Max message length</returns>
        public int GetMaxMessageLength(RSAParameters @params)
        {
            return m_OAEP.GetMaxMessageLength(@params);
        }
    }

    /// <summary>
    /// All custom signature providers must implement this interface.  The 
    /// RSACrypto class handles encryption and decryption of data.  The 
    /// SignatureProvider is intended to provide the hashing and signature
    /// generation mechanism used to create the comparison data.
    /// </summary>
    public interface ISignatureProvider
    {
        /// <summary>
        /// Generates a hash for the input data.
        /// </summary>
        /// <param name="dataBytes">Data to be signed</param>
        /// <param name="params">RSA Parameters used for signature calculation</param>
        /// <returns>Computed signature (pre-encryption)</returns>
        byte[] EncodeSignature(byte[] dataBytes, RSAParameters @params);
        /// <summary>
        /// Verifies the signed data against the unsigned data after decryption.
        /// </summary>
        /// <param name="dataBytes">Unsigned data</param>
        /// <param name="signedDataBytes">Signed data (after decryption)</param>
        /// <param name="params">RSAParameters used for signature computation</param>
        /// <returns>Boolean representing whether the input data matches the signed data</returns>
        bool VerifySignature(byte[] dataBytes, byte[] signedDataBytes, RSAParameters @params);
    }

    /// <summary>
    /// Uses the DER (Distinguished Encoding Rules) 
    /// and the SHA1 hash provider for encoding generation.
    /// </summary>
    public sealed class EMSAPKCS1v1_5_SHA1 : ISignatureProvider
    {

        //Default hash provider for hashing operations.
        private IHashProvider m_hashProvider;

        //Length of the hash generated by the hash provider
        private int m_hLen;

        /// <summary>
        /// Default constructor
        /// </summary>
        public EMSAPKCS1v1_5_SHA1()
        {
            m_hashProvider = new SHA1();
            m_hLen = 20;
        }

        /// <summary>
        /// Hashes and encodes the signature for encryption.  Uses the DER (Distinguished Encoding Rules) 
        /// and the SHA1 hash provider for encoding generation.
        /// </summary>
        /// <param name="dataBytes">Data to be signed</param>
        /// <param name="params">RSA Parameters used for signature calculation</param>
        /// <returns>Computed signature (pre-encryption)</returns>
        public byte[] EncodeSignature(byte[] dataBytes, RSAParameters @params)
        {
            //Set the intended message length (key length)
            int emLen = @params.N.Length;

            //Compute the hash of the data
            byte[] H = m_hashProvider.ComputeHash(dataBytes);
            //Get the digest encoding information for the hash being used.
            byte[] bytDigestEncoding = DigestEncoding.SHA1();

            //Create the hashed message including the digest info
            byte[] T = new byte[(bytDigestEncoding.Length + m_hLen)];
            bytDigestEncoding.CopyTo(T, 0);
            H.CopyTo(T, bytDigestEncoding.Length);

            H = null;
            bytDigestEncoding = null;

            if (emLen < T.Length + 11)
            {
                throw new CryptographicException("Message too short.");
            }

            //Create the padding string, octet string of 0xff
            byte[] PS = new byte[(emLen - T.Length - 3)];
            for (int i = 0; i <= PS.Length - 1; i++)
            {
                PS[i] = 0xff;
            }

            byte[] result = new byte[emLen];

            //Add the leading identifier bytes
            result[0] = 0x00;
            result[1] = 0x01;

            //Copy the padding string to the result
            PS.CopyTo(result, 2);

            //Add the separator byte
            result[PS.Length + 2] = 0x00;

            //Copy the digest info
            T.CopyTo(result, PS.Length + 3);
            PS = null;
            T = null;

            return result;
        }

        /// <summary>
        /// Verifies the signed data against the unsigned data after decryption.
        /// </summary>
        /// <param name="dataBytes">Unsigned data</param>
        /// <param name="signedDataBytes">Signed data (after decryption)</param>
        /// <param name="params">RSAParameters used for signature computation</param>
        /// <returns>Boolean representing whether the input data matches the signed data</returns>
        bool ISignatureProvider.VerifySignature(byte[] dataBytes, byte[] signedDataBytes, RSAParameters @params)
        {
            byte[] EM2 = EncodeSignature(dataBytes, @params);

            if (!(EM2.Length == signedDataBytes.Length))
            {
                return false;
            }

            bool isValid = true;

            for (int i = 0; i <= EM2.Length - 1; i++)
            {
                if (!(EM2[i] == signedDataBytes[i]))
                {
                    isValid = false;
                    break;
                }
            }

            return isValid;
        }
    }

    /// <summary>
    /// Uses the DER (Distinguished Encoding Rules) 
    /// and the SHA256 hash provider for encoding generation.
    /// </summary>
    public sealed class EMSAPKCS1v1_5_SHA256 : ISignatureProvider
    {

        //Default hash provider for hashing operations.
        private IHashProvider m_hashProvider;

        //Length of the hash generated by the hash provider
        private int m_hLen;

        /// <summary>
        /// Default constructor
        /// </summary>
        public EMSAPKCS1v1_5_SHA256()
        {
            m_hashProvider = new SHA256();
            m_hLen = 32;
        }

        /// <summary>
        /// Hashes and encodes the signature for encryption.  Uses the DER (Distinguished Encoding Rules) 
        /// and the SHA256 hash provider for encoding generation.
        /// </summary>
        /// <param name="dataBytes">Data to be signed</param>
        /// <param name="params">RSA Parameters used for signature calculation</param>
        /// <returns>Computed signature (pre-encryption)</returns>
        public byte[] EncodeSignature(byte[] dataBytes, RSAParameters @params)
        {
            //Set the intended message length (key length)
            int emLen = @params.N.Length;

            //Compute the hash of the data
            byte[] H = m_hashProvider.ComputeHash(dataBytes);
            //Get the digest encoding information for the hash being used.
            byte[] bytDigestEncoding = DigestEncoding.SHA256();

            //Create the hashed message including the digest info
            byte[] T = new byte[(bytDigestEncoding.Length + m_hLen)];
            bytDigestEncoding.CopyTo(T, 0);
            H.CopyTo(T, bytDigestEncoding.Length);

            H = null;
            bytDigestEncoding = null;

            if (emLen < T.Length + 11)
            {
                throw new CryptographicException("Message too short.");
            }

            //Create the padding string, octet string of 0xff
            byte[] PS = new byte[(emLen - T.Length - 3)];
            for (int i = 0; i <= PS.Length - 1; i++)
            {
                PS[i] = 0xff;
            }

            byte[] result = new byte[emLen];

            //Add the leading identifier bytes
            result[0] = 0x00;
            result[1] = 0x01;

            //Copy the padding string to the result
            PS.CopyTo(result, 2);

            //Add the separator byte
            result[PS.Length + 2] = 0x00;

            //Copy the digest info
            T.CopyTo(result, PS.Length + 3);
            PS = null;
            T = null;

            return result;
        }

        /// <summary>
        /// Verifies the signed data against the unsigned data after decryption.
        /// </summary>
        /// <param name="dataBytes">Unsigned data</param>
        /// <param name="signedDataBytes">Signed data (after decryption)</param>
        /// <param name="params">RSAParameters used for signature computation</param>
        /// <returns>Boolean representing whether the input data matches the signed data</returns>
        bool ISignatureProvider.VerifySignature(byte[] dataBytes, byte[] signedDataBytes, RSAParameters @params)
        {
            byte[] EM2 = EncodeSignature(dataBytes, @params);

            if (!(EM2.Length == signedDataBytes.Length))
            {
                return false;
            }

            bool isValid = true;

            for (int i = 0; i <= EM2.Length - 1; i++)
            {
                if (!(EM2[i] == signedDataBytes[i]))
                {
                    isValid = false;
                    break;
                }
            }

            return isValid;
        }
    }

    /// <summary>
    /// Base interface that must be implemented by all hash providers.
    /// </summary>
    public interface IHashProvider
    {

        /// <summary>
        /// Compute the hash of the input byte array and return the hashed value as a byte array.
        /// </summary>
        /// <param name="inputData">Input data</param>
        /// <returns>Hashed data.</returns>
        byte[] ComputeHash(byte[] inputData);
    }

/// <summary>
	/// Hash provider based on SHA256
	/// </summary>
	public sealed class SHA256 : IHashProvider
	{
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SHA256() { }
        
        /// <summary>
		/// Compute the hash of the input byte array and return the hashed value as a byte array.
		/// </summary>
		/// <param name="inputData">Input data</param>
		/// <returns>SHA256 Hashed data</returns>
		byte[] IHashProvider.ComputeHash(byte[] inputData)
		{
			System.Security.Cryptography.SHA256Managed x = new System.Security.Cryptography.SHA256Managed();
			return x.ComputeHash(inputData);
		}
	}

	/// <summary>
	/// Hash provider based on HMACSHA256 to allow inclusion of a hash seed value
	/// </summary>
	public sealed class HMACSHA256 : IHashProvider
	{
		private byte[] m_Key;

		/// <summary>
		/// Constructor accepting a private key (seed) value
		/// </summary>
		/// <param name="privateKey">Byte array containing the private hash seed</param>
		public HMACSHA256(byte[] privateKey)
		{
			m_Key = privateKey;
		}

		/// <summary>
		/// Compute the hash of the input byte array and return the hashed value as a byte array.
		/// </summary>
		/// <param name="inputData">Input data</param>
		/// <returns>HMACSHA256 Hashed data.</returns>
		byte[] IHashProvider.ComputeHash(byte[] inputData)
		{
			System.Security.Cryptography.HMACSHA256 x;

			if (m_Key == null)
				x = new System.Security.Cryptography.HMACSHA256();
			else
				x = new System.Security.Cryptography.HMACSHA256(m_Key);

			return x.ComputeHash(inputData);
		}
	}
	
	/// <summary>
	/// Hash provider based on SHA1
	/// </summary>
	public sealed class SHA1 : IHashProvider
	{
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SHA1() { }
        
        /// <summary>
		/// Compute the hash of the input byte array and return the hashed value as a byte array.
		/// </summary>
		/// <param name="inputData">Input data</param>
		/// <returns>SHA1 Hashed data.</returns>
		byte[] IHashProvider.ComputeHash(byte[] inputData)
		{
			System.Security.Cryptography.SHA1Managed x = new System.Security.Cryptography.SHA1Managed();
			return x.ComputeHash(inputData);
		}
	}

	/// <summary>
	/// Hash provider based on HMACSHA1 to allow inclusion of a hash seed value
	/// </summary>
	public sealed class HMACSHA1 : IHashProvider
	{
		private byte[] m_Key;

		/// <summary>
		/// Constructor accepting a private key (seed) value
		/// </summary>
		/// <param name="privateKey">Byte array containing the private hash seed</param>
		public HMACSHA1(byte[] privateKey)
		{
			m_Key = privateKey;
		}

		/// <summary>
		/// Compute the hash of the input byte array and return the hashed value as a byte array.
		/// </summary>
		/// <param name="inputData">Input data</param>
		/// <returns>HMACSHA1 Hashed data.</returns>
		byte[] IHashProvider.ComputeHash(byte[] inputData)
		{
			System.Security.Cryptography.HMACSHA1 x;

			if (m_Key == null)
				x = new System.Security.Cryptography.HMACSHA1();
			else
				x = new System.Security.Cryptography.HMACSHA1(m_Key);

			return x.ComputeHash(inputData);
		}
	}
}

#endif