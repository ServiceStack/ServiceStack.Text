#if SL5 || WP

// From: http://scrypt.codeplex.com/
// License: http://scrypt.codeplex.com/license

using System;
using System.ComponentModel;
using System.IO;

namespace ServiceStack
{
    /// <summary>
    /// RSA Cryptography class
    /// </summary>
    public sealed class RSACryptoServiceProvider
    {

        #region " PRIVATE VARIABLES "
        //this is the bitlength for P and Q.  A bitlength of 512 will result in 
        //a modulus of length 1024 (which is a 1024 bit cipher).
        private int m_bitLength = 512;

        private bool m_isBusy = false;
        private BackgroundWorker m_worker1;
        private BackgroundWorker m_worker2;
        private int m_primeProgress = 0;

        //Parameters
        private RSAParameters m_RSAParams = new RSAParameters();

        //Provider to use for adding / removing padding.  Default is OAEP
        private IPaddingProvider m_PaddingProvider = new OAEP();

        //Private to signal if a key exists

        private bool m_KeyLoaded = false;
        #endregion

        #region " PROPERTIES "
        /// <summary>
        /// Different versions of RSA use different padding schemes.  This property allows you to 
        /// set the padding scheme you wish to use.  If not set, the default of OAEP will be 
        /// used.  While PKCS1 v1.5 is supported, OAEP is the recommended padding scheme to use.
        /// You can create your own padding schemes by implementing the IPaddingProvider interface.
        /// </summary>
        /// <value>Padding provider instance</value>
        /// <returns>Current padding provider</returns>
        public IPaddingProvider PaddingProvider
        {
            get { return m_PaddingProvider; }
            set
            {
                if (value != null)
                {
                    m_PaddingProvider = value;
                }
                else
                {
                    throw new ArgumentException("Supplied value must be a new instance of a padding provider.");
                }
            }
        }

        /// <summary>
        /// Based on the padding provider, messages are stricted to certain lengths for encryption.  Also,
        /// ensure that the key pair has either been generated or imported.
        /// </summary>
        public int MaxMessageLength
        {
            get
            {
                if (m_isBusy == true)
                    throw new CryptographicException("Operation cannot be performed while a current key generation operation is in progress.");

                if (m_KeyLoaded == false)
                    throw new CryptographicException("No key has been loaded.  You must import a key or make a call to GenerateKeys() before getting this property.");

                return m_PaddingProvider.GetMaxMessageLength(m_RSAParams);
            }
        }

        #endregion

        #region " CONSTRUCTORS "
        /// <summary>
        /// Default constructor for the RSA Class.  A cipher strength of 1024-bit will used by default.  To specify 
        /// a higher cipher strength, please use the alternate RSACrypto(cipherStrength) constructor.
        /// </summary>
        public RSACryptoServiceProvider() { }

        /// <summary>
        /// RSA Class Constructor
        /// </summary>
        /// <param name="cipherStrength">
        /// Cipher strength in bits.  2048 is recommended.  Must be a multiple of 8.  
        /// Max supported by this class is 4096.  Minimum is 256.  Cipher strength only 
        /// needs to be specified if generating new key pairs.  It is not necessary to 
        /// know the cipher strength when importing existing key pairs.
        /// </param>
        public RSACryptoServiceProvider(int cipherStrength)
        {
            if ((cipherStrength > 4096) || (cipherStrength < 256) || (cipherStrength % 8 != 0))
                throw new ArgumentException("cipherStrength must be a value in the range of 256 tp 4096 and must be a multiple of 8.");

            //bitLength is used to calculat P and Q, so it needs
            //to be half of the cipherStrength.  bitLength 512 = 1024-bit encryption.
            m_bitLength = cipherStrength / 2;
        }
        #endregion

        #region " IMPORT / EXPORT FUNCTIONALITY "
        /// <summary>
        /// Return the currently loaded key as XML.  This method will automatically 
        /// return an empty string if no key has been loaded.
        /// </summary>
        /// <param name="includePrivate">Signals whether to include the private key in the output data.</param>
        /// <returns>XML String with the key data.</returns>
        public string ToXmlString(bool includePrivate)
        {
            //If no key is loaded, return an empty string
            if (m_KeyLoaded == false)
            {

                return string.Empty;
            }

            System.Text.StringBuilder sbKeys = new System.Text.StringBuilder();

            //Build the public key
            sbKeys.Append("<RSAKeyValue>");
            sbKeys.Append("<Modulus>" + Convert.ToBase64String(m_RSAParams.N) + "</Modulus>");
            sbKeys.Append("<Exponent>" + Convert.ToBase64String(m_RSAParams.E) + "</Exponent>");

            if (includePrivate == true)
            {
                sbKeys.Append("<P>" + Convert.ToBase64String(m_RSAParams.P) + "</P>");
                sbKeys.Append("<Q>" + Convert.ToBase64String(m_RSAParams.Q) + "</Q>");
                if (m_RSAParams.DP.Length > 0)
                {
                    sbKeys.Append("<DP>" + Convert.ToBase64String(m_RSAParams.DP) + "</DP>");
                }

                if (m_RSAParams.DQ.Length > 0)
                {
                    sbKeys.Append("<DQ>" + Convert.ToBase64String(m_RSAParams.DQ) + "</DQ>");
                }

                if (m_RSAParams.IQ.Length > 0)
                {
                    sbKeys.Append("<InverseQ>" + Convert.ToBase64String(m_RSAParams.IQ) + "</InverseQ>");
                }

                sbKeys.Append("<D>" + Convert.ToBase64String(m_RSAParams.D) + "</D>");
            }

            //Close the key XML
            sbKeys.Append("</RSAKeyValue>");

            return sbKeys.ToString();
        }

        /// <summary>
        /// Sets the current class internal variables based on the supplied XML. 
        /// Attempts to validate the XML prior to setting.
        /// </summary>
        /// <param name="xmlString">XML String containing key info</param>
        /// <remarks></remarks>
        public void FromXmlString(string xmlString)
        {
            RSAParameters oParams = new RSAParameters();

            using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(new StringReader(xmlString)))
            {
                int iNode = 0;

                while (reader.Read())
                {
                    if (reader.Value.Trim().Length > 0)
                    {
                        switch (iNode)
                        {
                            case 1:
                                oParams.N = Convert.FromBase64String(reader.Value);
                                iNode = 0;
                                break;
                            case 2:
                                oParams.E = Convert.FromBase64String(reader.Value);
                                iNode = 0;
                                break;
                            case 3:
                                oParams.P = Convert.FromBase64String(reader.Value);
                                iNode = 0;
                                break;
                            case 4:
                                oParams.Q = Convert.FromBase64String(reader.Value);
                                iNode = 0;
                                break;
                            case 5:
                                oParams.DP = Convert.FromBase64String(reader.Value);
                                iNode = 0;
                                break;
                            case 6:
                                oParams.DQ = Convert.FromBase64String(reader.Value);
                                iNode = 0;
                                break;
                            case 7:
                                oParams.IQ = Convert.FromBase64String(reader.Value);
                                iNode = 0;
                                break;
                            case 8:
                                oParams.D = Convert.FromBase64String(reader.Value);
                                iNode = 0;
                                break;
                        }
                    }

                    switch (reader.NodeType)
                    {
                        case System.Xml.XmlNodeType.Element:
                            switch (reader.Name.ToUpper())
                            {
                                case "MODULUS":
                                    iNode = 1;
                                    break;
                                case "EXPONENT":
                                    iNode = 2;
                                    break;
                                case "P":
                                    iNode = 3;
                                    break;
                                case "Q":
                                    iNode = 4;
                                    break;
                                case "DP":
                                    iNode = 5;
                                    break;
                                case "DQ":
                                    iNode = 6;
                                    break;
                                case "INVERSEQ":
                                    iNode = 7;
                                    break;
                                case "D":
                                    iNode = 8;
                                    break;
                            }
                            break;
                    }
                }
            }

            //If P and Q are set, set Phi
            if (oParams.P.Length > 0 && oParams.Q.Length > 0)
            {
                oParams.Phi = new BigInteger((new BigInteger(oParams.P) - 1) * (new BigInteger(oParams.Q) - 1)).getBytes();
            }

            if (Validate_Key_Data(oParams))
            {
                m_RSAParams = oParams;
                m_KeyLoaded = true;
            }
        }

        /// <summary>
        /// Import an existing set of RSA Parameters.  If only the public key is to be loaded, 
        /// Do not set the P, Q, DP, DQ, IQ or D values.  If P, Q or D are set, the parameters 
        /// will automatically be validated for existence of private key.
        /// </summary>
        /// <param name="params">RSAParameters object containing key data.</param>
        public void ImportParameters(RSAParameters @params)
        {
            if (Validate_Key_Data(@params))
            {
                m_RSAParams.D = @params.D;
                m_RSAParams.N = @params.N;
                m_RSAParams.DP = @params.DP;
                m_RSAParams.DQ = @params.DQ;
                m_RSAParams.E = @params.E;
                m_RSAParams.IQ = @params.IQ;
                m_RSAParams.P = @params.P;
                m_RSAParams.Q = @params.Q;
                //Phi can only be set internally and is always calculated
                m_RSAParams.Phi = new BigInteger((new BigInteger(@params.P) - 1) * (new BigInteger(@params.Q) - 1)).getBytes();

                m_KeyLoaded = true;
            }

        }

        /// <summary>
        /// Returns an RSAParameters object that contains the key data for the currently loaded key.  
        /// </summary>
        /// <returns>Instance of the currently loaded RSAParameters object or null if no key is loaded</returns>
        /// <remarks></remarks>
        public RSAParameters ExportParameters(bool exportPrivate=true)
        {
            RSAParameters result = new RSAParameters();

            if (m_KeyLoaded == true)
            {
                result = m_RSAParams;
            }
            else
            {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// Imports a blob that represents asymmetric key information.
        /// </summary>
        /// <param name="rawData">A byte array that represents an asymmetric key blob.</param>
        /// <exception cref="CryptographicException">Invalid key blob data</exception>
        /// <returns>Initialized RSAParameters structure</returns>
        public void ImportCspBlob(byte[] rawData)
        {
            using (var stream = new MemoryStream(rawData))
            {
                using (var reader = new BinaryReader(stream))
                {
                    BlobHeader header = BlobHeader.FromBinary(reader);

                    if (header.BlobType == KeyBlobType.PublicKeyBlob)
                    {
                        this.ImportParameters(PublicKeyBlob.FromBinary(reader, header).ToRSAParameters());
                        return;
                    }

                    if (header.BlobType == KeyBlobType.PrivateKeyBlob)
                    {
                        this.ImportParameters(PrivateKeyBlob.FromBinary(reader, header).ToRSAParameters());
                        return;
                    }
                }
            }

            throw new CryptographicException("Invalid key blob data");
        }

        /// <summary>
        /// Exports a blob that contains the key information associated with an AsymmetricAlgorithm object.
        /// </summary>
        /// <param name="includePrivateParameters">true to include the private key; otherwise, false.</param>
        /// <returns>A byte array that contains the key information associated with an AsymmetricAlgorithm object</returns>
        public byte[] ExportCspBlob(bool includePrivateParameters)
        {
            var @params = this.ExportParameters();

            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    if (includePrivateParameters) PrivateKeyBlob.FromRSAParameters(@params).ToBinary(writer);
                    else PublicKeyBlob.FromRSAParameters(@params).ToBinary(writer);

                    return stream.ToArray();
                }
            }
        }

        #endregion


        #region " KEY GENERATION "
        /// <summary>
        /// Generate the RSA Key Pair using the default exponent of 65537.
        /// </summary>
        public void GenerateKeys()
        {
            GenerateKeys(m_bitLength * 2, 65537);
        }

        /// <summary>
        /// Generate the RSA Key Pair using a supplied cipher strength and the default 
        /// exponent value of 65537.  If a cipherStrength was specified in the constructor, 
        /// the supplied value will override it.
        /// </summary>
        /// <param name="cipherStrength">The strength of the cipher in bits.  Must be a multiple of 8 
        /// and between 256 and 4096</param>
        public void GenerateKeys(int cipherStrength)
        {
            GenerateKeys(cipherStrength, 65537);
        }

        /// <summary>
        /// Generate the RSA Key Pair using a supplied cipher strength value and exponent value.  
        /// A prime number value between 3 and 65537 is recommended for the exponent.  Larger 
        /// exponents can increase security but also increase encryption time.  Your supplied 
        /// exponent may be automatically adjusted to ensure compatibility with the RSA algorithm 
        /// security requirements.  If a cipherStrength was specified in the constructor, 
        /// the supplied <paramref name="cipherStrength"/> value will override it.
        /// </summary>
        /// <param name="cipherStrength">The strength of the cipher in bits.  Must be a multiple of 8 
        /// and between 256 and 4096</param>
        /// <param name="exponent">Custom exponent value to be used for RSA Calculation</param>
        public void GenerateKeys(int cipherStrength, int exponent)
        {
            if ((cipherStrength > 4096) || (cipherStrength < 256) || (cipherStrength % 8 != 0))
                throw new ArgumentException("cipherStrength must be a value between 256 and 4096 and must be a multiple of 8.");

            if (m_isBusy == true)
                throw new CryptographicException("Operation cannot be performed while a current key generation operation is in progress.");

            m_KeyLoaded = false;
            m_isBusy = true;

            //bitLength is used to calculate P and Q, so it needs
            //to be half of the cipherStrength.  bitLength 512 = 1024-bit encryption.
            m_bitLength = cipherStrength / 2;

            //Make sure this is a positive number
            BigInteger iExp = new BigInteger(Math.Abs((long)exponent));

            //Make sure this is an odd number
            if (iExp % 2 == 0)
            {
                iExp += 1;
            }

            m_RSAParams.E = iExp.getBytesRaw();

            m_primeProgress = 0;
            m_worker1 = new BackgroundWorker();
            m_worker2 = new BackgroundWorker();
            m_worker1.RunWorkerCompleted += OnPrimeGenerated;
            m_worker2.RunWorkerCompleted += OnPrimeGenerated;

            Generate_Primes();
        }

        private void Generate_Primes()
        {

            m_worker1.DoWork += Generate_P;
            m_worker2.DoWork += Generate_Q;

            m_worker1.RunWorkerAsync();
            m_worker2.RunWorkerAsync();

        }

        private void Generate_P(Object sender, DoWorkEventArgs e)
        {

            DateTime dt = DateTime.Now;
            int iSeed = (dt.Millisecond * (dt.Second + dt.Minute) / dt.Month) * dt.Year;

            byte[] tmp = new byte[m_bitLength + 1];
            tmp = new BigInteger(BigInteger.genPseudoPrime(m_bitLength, new Random(iSeed))).getBytesRaw();


            m_RSAParams.P = tmp;
        }

        private void Generate_Q(Object sender, DoWorkEventArgs e)
        {
            DateTime dt = DateTime.Now;
            int iSeed = (dt.Year + dt.Second + dt.Minute + dt.Millisecond) / 3;

            byte[] tmp = new byte[m_bitLength + 1];
            tmp = new BigInteger(BigInteger.genPseudoPrime(m_bitLength, new Random(iSeed))).getBytesRaw();


            m_RSAParams.Q = tmp;
        }

        private void BuildKeys()
        {
            //Make a call to Generate_Primes.
            BigInteger P = new BigInteger(m_RSAParams.P);
            BigInteger Q = new BigInteger(m_RSAParams.Q);

            //Exponent.  This needs to be a number such that the 
            //GCD of the Exponent and Phi is 1.  The larger the exp. 
            //the more secure, but it increases encryption time.
            BigInteger E = new BigInteger(m_RSAParams.E);

            BigInteger N = new BigInteger(0);
            //Public and Private Key Part (Modulus)
            BigInteger D = new BigInteger(0);
            //Private Key Part
            BigInteger DP = new BigInteger(0);
            BigInteger DQ = new BigInteger(0);
            BigInteger IQ = new BigInteger(0);
            BigInteger Phi = new BigInteger(0);
            //Phi

            //Make sure P is greater than Q, swap if less.
            if (P < Q)
            {
                BigInteger biTmp = P;
                P = Q;
                Q = biTmp;
                biTmp = null;

                m_RSAParams.P = P.getBytesRaw();
                m_RSAParams.Q = Q.getBytesRaw();
            }

            //Calculate the modulus
            N = P * Q;
            m_RSAParams.N = N.getBytesRaw();

            //Calculate Phi
            Phi = (P - 1) * (Q - 1);
            m_RSAParams.Phi = Phi.getBytesRaw();


            //Make sure our Exponent will work, or choose a larger one.
            while (Phi.gcd(E) > 1)
            {
                //If the GCD is greater than 1 iterate the Exponent
                E = E + 2;
                //Also make sure the Exponent is prime.
                while (!E.isProbablePrime())
                {
                    E = E + 2;
                }
            }

            //Make sure the params contain the updated E value
            m_RSAParams.E = E.getBytesRaw();


            //Calculate the private exponent D.
            D = E.modInverse(Phi);
            m_RSAParams.D = D.getBytesRaw();

            //Calculate DP
            DP = E.modInverse(P - 1);
            m_RSAParams.DP = DP.getBytesRaw();

            //Calculate DQ
            DQ = E.modInverse(Q - 1);
            m_RSAParams.DQ = DQ.getBytesRaw();

            //Calculate InverseQ
            IQ = Q.modInverse(P);
            m_RSAParams.IQ = IQ.getBytesRaw();

            m_KeyLoaded = true;
            m_isBusy = false;

            OnKeysGenerated(this);

        }

        #endregion

        #region " EVENTS AND EVENT DELEGATES "

        private void OnPrimeGenerated(Object sender, RunWorkerCompletedEventArgs e)
        {
            m_primeProgress += 50;

            if (m_primeProgress == 100)
            {
                //Verify that P and Q are not equal...if they are, we need to regenerate Q
                //Handle the case where Q and P might end up being equal.  This will run 
                //the worker again using the same settings as before.
                BigInteger biP = new BigInteger(m_RSAParams.P);
                BigInteger biQ = new BigInteger(m_RSAParams.Q);

                if (biP == biQ)
                {
                    m_primeProgress = 50;
                    m_worker2.DoWork += Generate_Q;
                    m_worker2.RunWorkerAsync();
                    return;
                }

                if (biP < biQ)
                {
                    BigInteger biTmp = new BigInteger(biP);
                    biP = biQ;
                    biQ = biTmp;
                    m_RSAParams.P = biP.getBytesRaw();
                    m_RSAParams.Q = biQ.getBytesRaw();
                }

                BuildKeys();
            }


        }

        /// <summary>
        /// Delegate for OnKeysGenerated event 
        /// </summary>
        /// <param name="sender">Object</param>
        public delegate void KeysGenerated(Object sender);

        /// <summary>
        /// Fires when key generation is complete.
        /// </summary>
        public event KeysGenerated OnKeysGenerated;

        #endregion

        #region " ENCRYPTION / SIGNING "

        /// <summary>
        /// Encrypt input bytes with the public key.  Data can only be decrypted with 
        /// the private key.  If no PaddingProvider is set, the default padding provider of OAEP will be assumed.  To 
        /// specify a different padding algorithm, make sure you set the PaddingProvider property.
        /// </summary>
        /// <param name="dataBytes">Data bytes to be encrypted</param>
        /// <returns>Encrypted byte array</returns>
        /// <remarks>Key generation is CPU intensive.  It is highly recommended that you create 
        /// your key pair in advance and use a predetermined key pair.  If you do choose to allow 
        /// the key pair to be automatically generated, it can be exported to XML or an RSAParameter 
        /// set after the encryption is complete.</remarks>
        public byte[] Encrypt(byte[] dataBytes)
        {
            if (m_isBusy == true)
                throw new CryptographicException("Operation cannot be performed while a current key generation operation is in progress.");

            if (m_KeyLoaded == false)
                throw new CryptographicException("No key has been loaded.  You must import a key or make a call to GenerateKeys() before encrypting.");

            return DoEncrypt(AddEncryptionPadding(dataBytes));
        }

        /// <summary>
        /// Run the encryption routine using the private key for encryption.  While this may be useful in some 
        /// fringe scenarios, if simple verification is needed it is recommended that you use the Sign() method 
        /// instead, which signs a hashed version of your data.   If no PaddingProvider is set, the default padding 
        /// provider of OAEP will be assumed.  To specify a different padding algorithm, make sure you set the 
        /// PaddingProvider property.  
        /// </summary>
        /// <param name="databytes">Data to be encrypted with the private key</param>
        /// <returns>Encrypted data bytes</returns>
        /// <remarks>
        /// <para>This method uses the PaddingProvider for message verification.  To create signature 
        /// hashes, please use the SignData and VerifyData methods.</para>
        /// <para>Data encrypted this way can be decrypted using your PUBLIC KEY.  This method of encryption is meant 
        /// for verification purposes only and does not secure your data against decryption.</para>
        /// </remarks>
        public byte[] EncryptPrivate(byte[] databytes)
        {
            if (m_isBusy == true)
                throw new CryptographicException("Operation cannot be performed while a current key generation operation is in progress.");

            if (m_KeyLoaded == false)
                throw new CryptographicException("No key has been loaded.  You must import a key or make a call to GenerateKeys() before encrypting.");

            return DoEncryptPrivate(AddEncryptionPadding(databytes));
        }

        private byte[] AddEncryptionPadding(byte[] dataBytes)
        {
            return m_PaddingProvider.EncodeMessage(dataBytes, m_RSAParams);
        }

        private byte[] DoEncrypt(byte[] dataBytes)
        {
            //Validate the key data
            if (m_RSAParams.E == null || m_RSAParams.N == null || m_RSAParams.E.Length == 0 || m_RSAParams.N.Length == 0)
            {
                throw new CryptographicException("Invalid Key.");
            }

            return encryptData(dataBytes, m_RSAParams.E, m_RSAParams.N);
        }

        private byte[] DoEncryptPrivate(byte[] dataBytes)
        {

            //Validate the key data
            if (Validate_Private_Key() == false)
            {
                return null;
            }

            return encryptData(dataBytes, m_RSAParams.D, m_RSAParams.N);
        }

        private byte[] encryptData(byte[] dataBytes, byte[] bytExponent, byte[] bytModulus)
        {
            //Make sure the data to be encrypted is not bigger than the modulus
            if (dataBytes.Length > bytModulus.Length)
            {
                throw new CryptographicException("Data length cannot be larger than the modulus.  Specify a larger cipher strength " +
                                            "in the constructor and generate a new key pair, or consider encrypting a smaller " +
                                            "amount of data.");
            }

            BigInteger oRawData = new BigInteger(dataBytes, dataBytes.Length);
            BigInteger result = oRawData.modPow(new BigInteger(bytExponent), new BigInteger(bytModulus));

            return result.getBytesRaw();
        }

        /// <summary>
        /// Sign a hash of the input data using the supplied Signature Provider and encrypt with the private key.    
        /// </summary>
        /// <param name="dataBytes">Data to be hashed and signed</param>
        /// <param name="signatureProvider">The signature provider to use for signature generation.</param>
        /// <returns>Signed hash bytes</returns>
        public byte[] SignData(byte[] dataBytes, ISignatureProvider signatureProvider)
        {
            if (m_isBusy == true)
                throw new CryptographicException("Operation cannot be performed while a current key generation operation is in progress.");

            if (m_KeyLoaded == false)
                throw new CryptographicException("No key has been loaded.  You must import a key or make a call to GenerateKeys() before performing data operations.");

            //Key validation is done in the DoEncryptPrivate method.
            return DoEncryptPrivate(signatureProvider.EncodeSignature(dataBytes, m_RSAParams));
        }

        #endregion

        #region " DECRYPTION / SIGNATURE VERIFICATION "

        /// <summary>
        /// Decrypt data that was encrypted using the Public Key.  If no PaddingProvider is set, the default
        /// padding provider of OAEP will be assumed.  To specify a different padding algorithm, make sure 
        /// you set the PaddingProvider property.
        /// </summary>
        /// <param name="encryptedBytes">Encrypted bytes</param>
        /// <returns>Decrypted bytes</returns>
        public byte[] Decrypt(byte[] encryptedBytes)
        {
            if (m_isBusy == true)
                throw new CryptographicException("Operation cannot be performed while a current key generation operation is in progress.");

            if (m_KeyLoaded == false)
                throw new CryptographicException("No key has been loaded.  You must import a key or make a call to GenerateKeys() before performing data operations.");

            byte[] em = DoDecrypt(ref encryptedBytes);

            return RemoveEncryptionPadding(em);
        }

        /// <summary>
        /// Decrypt data that was encrypted with the Private Key.  NOTE:  This method 
        /// uses the PaddingProvider for message decoding.  To create signature 
        /// hashes, please use the SignData and VerifyData methods.  If no PaddingProvider is set, the default
        /// padding provider of OAEP will be assumed.  To specify a different padding algorithm, make sure 
        /// you set the PaddingProvider property.
        /// </summary>
        /// <param name="encryptedBytes">Encrypted bytes</param>
        /// <returns>Decrypted bytes</returns>
        public byte[] DecryptPublic(byte[] encryptedBytes)
        {
            if (m_isBusy == true)
                throw new CryptographicException("Operation cannot be performed while a current key generation operation is in progress.");

            if (m_KeyLoaded == false)
                throw new CryptographicException("No key has been loaded.  You must import a key or make a call to GenerateKeys() before performing data operations.");

            byte[] bytEM = DoDecryptPublic(ref encryptedBytes);
            return m_PaddingProvider.DecodeMessage(bytEM, m_RSAParams);
        }

        private byte[] DoDecrypt(ref byte[] encryptedBytes)
        {
            if (Validate_Private_Key() == true)
            {
                return decryptData(encryptedBytes, m_RSAParams.D, m_RSAParams.N);
            }
            else
            {
                throw new CryptographicException("Invalid Key.");
            }
        }

        private byte[] DoDecryptPublic(ref byte[] encryptedBytes)
        {
            if (m_RSAParams.E.Length == 0 || m_RSAParams.N.Length == 0)
            {
                throw new CryptographicException("Invalid Key.");
            }
            return decryptData(encryptedBytes, m_RSAParams.E, m_RSAParams.N);
        }

        private byte[] decryptData(byte[] dataBytes, byte[] bytExponent, byte[] bytModulus)
        {
            BigInteger oEncData = new BigInteger(dataBytes, dataBytes.Length);
            return oEncData.modPow(new BigInteger(bytExponent), new BigInteger(bytModulus)).getBytesRaw();
        }

        private byte[] RemoveEncryptionPadding(byte[] dataBytes)
        {
            return m_PaddingProvider.DecodeMessage(dataBytes, m_RSAParams);
        }

        /// <summary>
        /// Verify the signature against the unsigned data.  The encryptedData is decrypted using the public key and 
        /// the unsignedData is hashed and compared to the un-encrypted signed data using the supplied SignatureProvider.  
        /// </summary>
        /// <param name="unsignedData">The raw, unencrypted data to be hashed and compared.</param>
        /// <param name="encryptedData">The data that has been hashed and encrypted with the private key.</param>
        /// <param name="signatureProvider">The signature provider that matches the algorithm used to generate the original signature</param>
        /// <returns>Boolean representing whether the signature was valid (verified)</returns>
        public bool VerifyData(byte[] unsignedData, byte[] encryptedData, ISignatureProvider signatureProvider)
        {
            if (m_isBusy == true)
                throw new CryptographicException("Operation cannot be performed while a current key generation operation is in progress.");

            if (m_KeyLoaded == false)
                throw new CryptographicException("No key has been loaded.  You must import a key or make a call to GenerateKeys() before performing data operations.");

            return signatureProvider.VerifySignature(unsignedData, DoDecryptPublic(ref encryptedData), m_RSAParams);
        }
        #endregion

        #region " VALIDATION "

        private bool Validate_Key_Data(RSAParameters @params)
        {
            bool result = true;

            //Make sure the public bits have been set
            if (!(@params.N.Length > 0))
            {
                throw new CryptographicException("Value for Modulus (N) is missing or invalid.");
            }

            if (!(@params.E.Length > 0))
            {
                throw new CryptographicException("Value for Public Exponent (E) is missing or invalid.");
            }

            //If any of the private key data (D, P or Q) were supplied, validating private
            //key info.
            if (@params.D.Length > 0 || @params.P.Length > 0 || @params.Q.Length > 0)
            {
                if (!(@params.P.Length > 0))
                {
                    throw new CryptographicException("Value for P is missing or invalid.");
                }

                if (!(@params.Q.Length > 0))
                {
                    throw new CryptographicException("Value for Q is missing or invalid.");
                }

                if (!(@params.D.Length > 0))
                {
                    throw new CryptographicException("Value for Private Exponent (D) is missing or invalid.");
                }

                //Validate the key
                if (@params.P.Length != @params.N.Length / 2 || @params.Q.Length != @params.N.Length / 2)
                {
                    throw new CryptographicException("Invalid Key.");

                }

                BigInteger biN = new BigInteger(@params.N);
                BigInteger biP = new BigInteger(@params.P);
                BigInteger biQ = new BigInteger(@params.Q);

                BigInteger tmpMod = new BigInteger(biP * biQ);

                if (!(tmpMod == biN))
                {
                    throw new CryptographicException("Invalid Key.");
                }

                tmpMod = null;

            }

            return result;
        }

        private bool Validate_Private_Key()
        {
            bool result = true;
            //Make sure a private key is set
            if ((m_RSAParams.N.Length + m_RSAParams.E.Length + m_RSAParams.P.Length + m_RSAParams.Q.Length + m_RSAParams.D.Length) == 0)
            {
                throw new CryptographicException("Invalid key");
            }
            else
            {
                //Make sure P and Q and the Modulus are correct
                //Validate the key
                if (m_RSAParams.P.Length != m_RSAParams.N.Length / 2 || m_RSAParams.Q.Length != m_RSAParams.N.Length / 2)
                {
                    throw new CryptographicException("Invalid Key.");
                }
            }

            return result;
        }

        #endregion

    }
}

#endif