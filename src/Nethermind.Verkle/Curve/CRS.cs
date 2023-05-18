// Copyright 2022 Demerzel Solutions Limited
// Licensed under Apache-2.0.For full terms, see LICENSE in the project root.

using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Nethermind.Verkle.Fields.FpEElement;
using Nethermind.Verkle.Fields.FrEElement;

namespace Nethermind.Verkle.Curve
{
    // ReSharper disable InconsistentNaming
    public class CRS
    {
        public readonly Banderwagon[] BasisG;
        public readonly Banderwagon BasisQ;

        public static CRS Instance { get; } = new CRS(CrsStruct.Generate());

        private CRS(Banderwagon[] basisG)
        {
            BasisG = basisG;
            BasisQ = Banderwagon.Generator;
        }

        public static CRS Generate(long numPoints)
        {
            const string seed = "eth_verkle_oct_2021";
            Span<byte> seedSpan = Encoding.ASCII.GetBytes(seed);

            Banderwagon[] points = new Banderwagon[numPoints];

            ulong generatedPoints = 0;

            ulong increment = 0;
            while (generatedPoints != (ulong)numPoints)
            {
                byte[] toHash = new byte[seedSpan.Length + 8];
                Span<byte> toHashSpan = toHash;

                seedSpan.CopyTo(toHashSpan);
                BinaryPrimitives.WriteUInt64BigEndian(toHashSpan[seedSpan.Length..], increment);

                byte[] hash = SHA256.HashData(toHash);

                FpE x = FpE.FromBytesReduced(hash, true);
                increment++;

                byte[] xAsBytes = x.ToBytesBigEndian();
                (FpE X, FpE Y)? pointFound = Banderwagon.FromBytes(xAsBytes);
                if (pointFound is null) continue;
                points[generatedPoints] =
                    new Banderwagon(null, new ExtendedPoint(pointFound.Value.X, pointFound.Value.Y));
                generatedPoints += 1;
            }

            return new CRS(points);
        }

        public Banderwagon CommitSparse(Dictionary<int, FrE> values)
        {
            if (values.Count == 0) return Banderwagon.Identity;

            List<Banderwagon> points = new(values.Count);
            List<FrE> scalars = new(values.Count);
            foreach (KeyValuePair<int, FrE> keyVal in values)
            {
                points.Add(BasisG[keyVal.Key]);
                scalars.Add(keyVal.Value);
            }

            Banderwagon commitment = Banderwagon.MultiScalarMul(CollectionsMarshal.AsSpan(points), CollectionsMarshal.AsSpan(scalars));
            return commitment;
        }

        public Banderwagon Commit(FrE[] values)
        {
            Span<Banderwagon> elements = BasisG;
            elements = elements[..values.Length];
            return Banderwagon.MultiScalarMul(elements, values);
        }
    }

    public struct CrsStruct
    {
        private static readonly string[] _constants =
        {
            "01587ad1336675eb912550ec2a28eb8923b824b490dd2ba82e48f14590a298a0",
            "6c6e607df0723edfff382fa914bfc38136f3300ab2e06fb97007b559fd323b82",
            "326be3bebfd97ed9d0d4ca1b8bc47e036a24b129f1488110b71c2cae1463db8f",
            "6bd241cc12dc9b2c0ad6fc85e016605c49c1a92939c7faeea0a555d2a1c3ddf8",
            "00d4bb940478cca48a5b822533d2b3215857ae7c6643c5954c96a0084ebffb2c",
            "1c817b76e1c869c4a74f9ce5b8bc04dc810dae7a61ee05616a29eca128e60d3b",
            "03ef64cbed1a63b043942bd0b114537227a116ffadd92a47749460b8facc7af9",
            "1436bda962957699c4d084acd6964db917c46b6c9a42465f9f656c58def17e84",
            "02fccde8e9b11a8d34bc1cddb50aca2d9158d8d3a8ced807020f934931ac6095",
            "45097b0216b48412d811c2c0e7c5f58aba24abdda30b6aa54eae160e10943df0",
            "030d1cb4f9ef18bcb750f5cbb930fd4898159e92bd885064b928cd30aaa54ce5",
            "6e1f15bfd5f168bf7ec2b7387c32cd01f71ebd25e71bc4fc1cba1b44f4ad9151",
            "6d1e84c32bbbabaf2217ab4bdd8646b03aae580665ac8c6a769bcb3d92a721bb",
            "12de72bbaa1bfa8746da284896e33a4a7923ec63190a91e4c5bfa8c61333e82b",
            "58ff79a3f9de08fb1143897387e37f63549ea5f75b9219d7baf450ef30d5d112",
            "10a86f813de1dd939baf1dac5cccefe84a6999971ba8128bf343730d3baada33",
            "2cdd950bb4f7517907136dfd2b11d284b3716f825407e606a6ad10cd704496f6",
            "096b25c16436a6cae0471eb90a9040ff7398623c908b632d279590baabd9dcf0",
            "0d0718d3144eb6e464cb451654456abc8a53ed60c4a08c5b89f159cb89b9196e",
            "1b15d8ce0e81d8239fd9e7e879c425151ad4f9385c27461c8cafe256b11b17ed",
            "315986fdd301d938d645c9260b8b82b52bfdc9af2bd74d0c32af15ee40440648",
            "601ad2cc66d1f6284d7f5aa45108f4b65fbd3e51af4f42551c0b8da5b6a966cc",
            "7287651bc7e91729679d51880d4b1716260f6e5206c5809b2826fba169adfb10",
            "11069b14788323d1cf8aa8f0ae5dadb59d169798e8cb2f32085ecf7fcdfca032",
            "36ddf8420b56471d32a6515c41d7a2183095c2ec16f4af3a049a3704f4337ed8",
            "2a09bdf48e7536bb89a2bfd8e6cbe0591b36492a91463f7926f8cd46323a413c",
            "080db970eeb2731b8b63bef3241b6f260336a8616aa87adaf49fd36936a6cd58",
            "71d838cf0c0676403c4d88821453374d6d3846b7b9d9b324ec7b449c53a82569",
            "3d2b693c5f53a9fdec6c24f453612d64f5ad9829e3f3e59a6d7876863ed59935",
            "2cd4b950eff95da8bfd3a99f9a641fddd4fe9eee566e677060de372ce94fb668",
            "159c2e8a84619f89fdc69b50bc72adaab4dffc8bb3bab2063f934f8433910967",
            "1b010d7e6d5bb2a62b33441adc5502235473abeeca79d2f51e19fa59c36d94a0",
            "630250699a98b71daf58beb402855b7e8435e2b4f6b8d536f853097968a8a3ac",
            "14d67ec31da79b11b0a7e889df43d78635470762f6ea887d4d291853946c72f6",
            "5a9f7c3efdc3fe1232e173c9a89b59d671fde577df65a0a53a41ab382990cfb2",
            "60de5069e491197ccfd36f61bb08933b843632b101dbdf89bcb3b1a6582c65bb",
            "0ffc1b01ed4abebe9cdcff6d54ef4a50b3024e5300d9d04d60565e26dfe84a84",
            "36e472f20c57e33884a92932eeb0cccf11e7dd0f5d3ec586cc92cf4d879d22de",
            "60ece62788e2959caf2e3ab998368626f7e40b16b30f01201b901ecffe75cc21",
            "1985f6b39cfe5f8bf82cfd1a5eab5d64e88519f7fb70bee403bc3f57887f2c4c",
            "24e47c2cf038bfd163119ac42af80db88ec4b4b565a186e8fdaad21a11e14962",
            "4fbf63649d94501aba5b5d61a98315cc9c8f9f423d02d470e0e5112e12a8150a",
            "7088db054ee2fc59a2c46ad206424f52ce1f948b8c1e0b13be9f0008b83c2684",
            "475b3363f1995805b2863e39aab9274aabd0c363baff86eab63f45da7d317b50",
            "3c5d09f29829fbd824b6b3207a25e9b9bb0c3df9a729b7a69ab7488665c8a27e",
            "17f893a8a9a37ae6e87eecfcea499744171065e13ca31758678c1cc4bc4cd118",
            "2008e6fe70a032b2ba7b424713d9b0499234f0c5e432532f4aaac1bdfebfa0c4",
            "22896e61b3c70126c77b933a118fc3a542313aeb56237e0eaec854690a5287b1",
            "5331aaca6128b13f1dbee4451d8dc0d7c2bfcd070464860ad9f3d91f852f239a",
            "0d0bdf6c928de23d8c77bb26ec7092e2cf027c4c47c29929f9ca1ac0c78a8d65",
            "2865e040f90f5cbdb5ef1bfa760720510100d007601353b9a95b1ddf17e8055f",
            "00eb8c32b10a519e5d2817d27e6190fcf345cad20ec050d146394b60b5b4b85f",
            "5ab06cac8838b69af2f070703a64c1ba235fa72bac23a6ad55daf52776fdc41c",
            "18ad62b530caf917f3711e4382c0bf042572b730f98e090956694d6753d038fb",
            "007ba09d58d1bfe0c68a1ae56144a2ddc0ea45e45b92362864bb4e66ee803a35",
            "12eb3324b9eb3ee5b8f6f325ab1960aeeab38756b16fba938f03bcbb452c4dd3",
            "1ef567bba47dce5409605c3327dd7f40048fbdc96fa1d3ca8185f0948b9b32e6",
            "137b4c3f7291c7e645b1eed6cea3d41f0a2591cc19a69ab160581b30687ec914",
            "506a4fce2594d31c091af8205e802a63194f94336743331d0b0bd85283560839",
            "2b0a11fe0214ca8f89b0893719b2d3c80637da15ab9d0816a7650ad55ff89707",
            "16030ee7cf67a9f4f7f4680d2933db53d4da4f61f9b9d286a28d732e0fc4acc1",
            "2c924cc062c25084fcb7f9f3f6a430532543d55ec34c65ef7243ac769b02d055",
            "3b246e3b2d6adb4c44066e337be5c619a5db7deed74e616c872ac3446157373d",
            "15b8603963502ee41063f8ac0c4bf8abcafdf32a76b6ee4a10bbb2f059d5cb78",
            "0f35c60bf844d11a9b59c41bffd2cddbad6bf3bdb9a98d5361a6e565cccff210",
            "5041f34b1f0315ee5444371c7359d8b9d2603862317522cc1a3104bf1d344598",
            "2a03f98965860d41b57ff653a79d1cca5ad175e0d663162fac8c9c686f0a9524",
            "3d657e87e76c1451f4de5d3adc358d0f02a16c22eb7325bf532a8f94b0c3fc77",
            "4e6a29a68db24b64ea7a88557b014cc13ad7ea587cc275ab459f3003ffb14fe5",
            "4346c2572310deb3bae7423ac8cba8880870074d9ceab22d9d12aca673e3f125",
            "135c5e1f68e4e8dd428479f7af24c4bb3b05797ef2422d065acc528d8f3edae4",
            "55a10117b0f6fb62365740a28436154d3fcb8d808271485779ecb7a53a6810a1",
            "0bb7a17eecf2f3b6e3fbef4aba8ec0ec4c8d7fba5e36b5df21ec7c202e56a421",
            "433c8740ae66d63da8260680c238baa45fe78673f75d6e378db418c36845ebec",
            "6ebb41f882b5b63a463bd30ce539de07a407185cbea61b536f790439aa007782",
            "58755fa8ad0c3622a5b21f4a8af98fb56a810155418a3b8ecc2927bb338f1d98",
            "49f4ce81f67c70a39d61bdf0a7f5e234ba051a22425f240928aa1fa0bb2da37c",
            "704073de3a913284db276a2d6e7684d5f354f544a4cb010aaac22de250a2b0da",
            "082b13ea0d8ee7b81bdf4652ea5075ecf0df43946ab4f20746a6429cdb23b8d2",
            "59f757e82c95b58e8a2fe4a785833ff56d9e31018d1f30d693fdfadc99f3c0f4",
            "42de618b236c4c21f07c16925ca48e8e87bc3a865523314e6f1634d494ab5764",
            "0d9bd9c659fd684267dce3a3c610a38668de6499d0dd63329ea9358981c4cd40",
            "1a2cd1578aa271af75dcc5c6e3ba9f9689a53a337966aacc80fdb0627611b00d",
            "44f1df3e8f6b83456028cd92869b0ea08f88933583adec4f039e31d7c370b0e8",
            "19c775e0a3b031512de26fd92f39f2a63d47ca3fff1a060585e0bc1b7ffea14f",
            "64997ad62e83800f29e86a15edbebe433e371315089be903af5165dc531ba638",
            "11f9d0075c0d68028efa5d4ee44931902aae502993f87dc33b795a6951334f81",
            "0a3595d5c1768c6b9ebf58d0812486a7a2b6ee3ba09a7451c2640d870f0fdd44",
            "514533b154241b9a8aa569dcdaf0ea0c47a202e84379f0f96f0ed2b0737ab974",
            "40311eddca1cf52fffcb576facdd6782c14177b84f4c1fa2cd5153651cf3eb58",
            "11047e6ebd4657d9d97bf6310bcfdf0f16bcdfa71a9a8423e274954ca3b2ecd7",
            "0e54bd8008eb33ffb2d6a9aa3a1f217d08df20808af35d151f5747b5b661c8ad",
            "186ff1f38c263535fd7940840b201dba030244c365673265207fca9dfb28e70a",
            "1c63798cac808e95eaab13c948ff9467f157e05c61d676f98c4628d4010e4f43",
            "50ef869e219a20f9a9d240200dbaa9aa711d2872863d953d03ee5f5200a8ca23",
            "2f6f129bea8c428c916349ef17a1dc45f57e7b676d47064c1cb523f861bc74b8",
            "3709e17535d5dd41f041229b56e7fdc9c2e46ce702ee0892b18ba76ee6a176a3",
            "2dad0ccbbe224037d85bb2e50b5b247d004d5871d6cfb5ff483cce0e0eff434d",
            "428b6a319e20a7f027e899214b0f2489104bfb9d83be70f69dc21f1e870ff9e3",
            "050c8d9c558e9acdbb229eca58537e60f1d9844a7b0259b55015dfca8b9f8dfa",
            "64ee46fee1b11aacba3b5721d0e38f14d5bcf9c86532b0cf768427e6da0ff6ce",
            "1d11c158db20f0b7644dd6a763d0e3d87e14d12ed0e55f14f9e2d42076d482d9",
            "23ceda31ed151ec91f00e95ed850f2df286cc3c27b3fe21cdacecf7250f6a154",
            "48ee49b3077c39d7e04a06865e2bc8bbf101a52dd1f8a2ebf8681e046f685c30",
            "4ca1e6d31c2a710748b5ad3ae23d84b3ecd5c27d1f3bdcbe696c87327b0dffde",
            "0a27d824c94b8fb319463a65039e5d52088de414854a509b87f0b605160c18aa",
            "4fd708c31872b8ff22e1dfcf84859686142baa987a9864ca4ac490cfaee69c29",
            "23b3d10cc68896d54e2f33c445b1c798480e09a2cd37c9b28c4c91a056c141d2",
            "508d5d7f2bc5c1c6a4c627da8611d49266a8a13cca901b5428c8ed0804dfa02f",
            "3bda5ab1eee6c1914509576639c70debfe0b89c77fd8aadc2479f8759d04dd2d",
            "48496d7c5c81cdc1ce5e72ad208eaf43742448ad5f956f2505f2c51b1554c28d",
            "115f36176d4893d4fd8f7fd1095315c8e4568920abf58d879348e18b85f6c745",
            "00c03cd6a6e128a847da5fedbfe3a702198bbf6b2f0dedd9923bc27648dbf819",
            "44d7af44835986af758e004c7d280a2a2dec80060f5f4491a60741fcd2e5d5d3",
            "73309c645303c93b14bb6563427e112654f5bac892731d390fed370b6484eba8",
            "06dd88ef4f7f07f0561d4f42800496c66cbf459a3b16a54631fbc2e587f8ed1d",
            "3cfafdbaf2a2674e1f69fddf5f2069855659d429acb2fd7f37de727c15a64d69",
            "34d9814dfd7829279445a2190b76bacf3ab2d5c76d5df95a56b6e4f744c73043",
            "251bea277699d5cd2de85d9c8d85662a0a300bf27375d296e9d2f8821828788b",
            "3b5d9d6b866f2a902c9072c6df6a276c3c3ba26888df4abbdea906e636fe36d1",
            "26a0a9722f85d2491eb8b5659e7fe47ddfff184de9b25106b99a9f6b318317d7",
            "54d18a073d1512d6c7cd656862852e5a3d86570f19c9be9e499bb8445f6e14ad",
            "17fd16c6a125a0cfd30a614273426d653fe685aafe8eb4302dc47e96adcaa786",
            "0c58929d68c96b280fa07ab69c9f5c3b32d0dcb1df1036cc9fa6bd988879ad10",
            "41368b7919d13d794b2708a4d8e269ac9b5e1be808c4fabcc33e1cff60229262",
            "63b77d63949c456dc7e267811990968482935580b6ea1232f1f3bba6fb9939db",
            "4e3a70add9ef5995f4308f6ac84a1b116dc691353339cd73a68a540a0ff671af",
            "1ba2e7164638d8c52a62f057a042ed36c721c2d3d48024e7b88f9bc46f10aa0f",
            "3d9fd3f8d73e9828515789cd6fada61bd1412b9c5df04846b27cea5e71a88cbc",
            "3035cc3ec4132673e2cb122ebc106ac594565621297060d1ecdca09a4c4fe9fd",
            "2046c451b27632cc70f472db63965af4958f92566cf4feffafb3be2c2a4a25ea",
            "41a88014d1dbf54ac2794f466890e76fbbde569d59d5cf2a81ef7c6d2e7fb4f8",
            "219c516b84fc36a6f72c9676c0519d0e69327feaf0bd9bf936ba84c67ea13a7b",
            "604ae691b325846d0a22052567465e45a77372df7d4f7958cb6f354d05bd4f0b",
            "46aaa44fd28c2931cadf19cd42b935a4defb64ee458554cce060297fd971fd61",
            "195ea63d4029fa63f7d0bdecd800e8089f51bcf4b4ae3fa3f01548d106d970b2",
            "47d46db8f08d392710eb81268afaf3a7c76643933541ab0bc7b8d7d896c233ce",
            "2f0f0b868d0fe2273d2a6cc001668433488a300d72e05b8fa96c9617cdd862a9",
            "075fbd6d06102b9db5a28346ac5497479380e2e14f824ae1179b1431385ffa75",
            "3ecd49d5efe96264919ed91e67b93b140421068d5867a4381b76a428b1faf1bd",
            "36d75eab0a84b56f3676ef31df9e773b62d133f6a333a876e932fc9a3c8c5902",
            "17d27cfbe6948c998cf2ccce631d0b757fdc582a19b369ab59cd13e70b673dad",
            "03555faf0ef948b04e81f8a4ae83b0e91294f1ec77e6ea25dc25d3eac4f2e223",
            "149926c2c6b43aec4823e802502b6d869b2ed41cfd5b7bedfafbfe8248916505",
            "231e5e8c2bf748e8ddb4f69930e28cd34d5a27a71cd56fd23a10da005ae36631",
            "2d124e88703b6930e8d9b4774e3960819fe5a5741d98b9b77fbe4fa9be2935ca",
            "6a34e3d7cb26ae25ee613f2bec202316e245d7c683f3dc1d07500d291ce284db",
            "6627f0726e74c4a5bfa87d29faf50aaadf0b5db1fd7d749a8df4c305beb496b1",
            "646d5c4eef0094ba762917d2f97561772309a29f9ea4756992b852b42808c3d1",
            "601bca297fa3ae04e8f1985a7167c13c8484eb4325a7e38c3070d4ee522e15d0",
            "0daf2172ba56968161420862195ed970d5216b6ab8f7e7cb2dd67bf44dfe6981",
            "3204413ac07f912680eb3546429d299df3aae72964611be4ecaf298e7c726fb5",
            "375f07c94d8ef0b2874db19894dc1496be108ab7e57a7c826c6a1a9cb721a1ed",
            "6fb3b108da52da27cf9db5e90ccd7b8c2d99cb0a5abdab2c2ce59432b70f1e6b",
            "1261cc658e4029987ad630576e839859b7c2c7a3345df433e5092073ef885df8",
            "35da898a770f0455c5e1fb8f6a774f0238a1117031aef161333107e48abb8a95",
            "542e7218fd06387d31dcea617467cc6d2b05f8f25ab73845271bb08c0d518f0f",
            "131d5e225d83d93a5ef5f50201cc0ad2a1057b905b1c5bf8025b554a6d93827d",
            "3b884e6f3b1fe3e20bc4e9cde558561ee9ef3ae9791bade9c26f1cc7aae1a631",
            "4635d7803290a04e077a77bfabba5fc1c210ee73ac82c0dfa6991f60b8d430c4",
            "6e66740437747f4c2a860d0ea9dd4f2f22e23040d19d9514c98e5868eaa0278c",
            "3d22771ea26538838d100e461be75afef7be0a0114cea0cb2925fe2fea90ee2d",
            "66c5efb89ad3e45f45a55db5222bb1a4b211fb068fe356412426e2014ed10fef",
            "3bd0683c1a7194a6e4ced18638440678d46552fb35c383abf56bd586963a5c8e",
            "5afd6c161bf3d724e39f803a9c5d81c23d26759a11dab77989b552d173206c53",
            "5dbb14e4f27e63b6b5a1522e8bead57504be14882c666c41757d1101554a742b",
            "15141ec4b230539ec96f4e714f8ed73bad901f6d6727fd3d54c5eb27a4fd3cdd",
            "34623c495135e4e17db5b161a4961e1c330f429584ceead4c6afd23b8ac13f37",
            "20fc3086ecdffaaed0c96d06898e8187dc17c85b29d38c27a3af5cc4f76aff94",
            "737b6dcc3ec92db01c53b86e441ea8a4b7874bcc1fbffa11ffb779bf82736376",
            "0d3c1364c75217240ec411f2ae953222028fabaf482d6cf53241662fc557e9b9",
            "0f58a25ceb34fbd33860ac2ae0f0ccc5529ae264fe57f2ef5e3e60623a71bc53",
            "66525b282e33b46abe509d2873e29b6467040d23b756d344331a73f2bef51fdc",
            "0652b5522405067e6f63cbae0fed41b809052d2a56535059f3ac8b36d2c4e585",
            "657c4a8cc8cb80ef24fc4b2834041eff4dcaa4cc40aefbe2f5bef0e87eb5a4e1",
            "11ba12dcc5f5396889dc4be56ab1f25b1a0d76e430d901f22f0ba9c25678c650",
            "57a5dc711949cf924f38a734c07a8a8643b1430b488e7cd78ebca551adfbf2dd",
            "059fccd2e8d157dd85aa7eaa310b75c963a0e351027dd0b9b6460954d1f46d09",
            "5ac4d9f59294968d6edcfcba69b7d15e678a952b8ba999432bdc8ec2843577b8",
            "0dd31b7b868dc5d934da5fda8cfd991eb4aa664ba6e751e28cc7f6691b3f0c13",
            "03aba75fc6860f3240ffc97b8912fc8d384f96ef0dee347e28271781661715a7",
            "5b45366b3a0a22016e6b690590ff9c9fa816cee3fbaded5348b93d892c4326b1",
            "2da06dff2de37f57204ca55cc8a367fcead1c6718243a39935dda23936497014",
            "56f9004588581db153e4eecc057cdc988d3dec2d5337bbe4da7feb4c919f1885",
            "2edfe2f0fe0cb4c784473faec57c1d9fd2ee23fcf2488d8f2c18db168d0709fe",
            "290f8d0473233dad9322c3c563bdfbafe730432726427592ebb2ba2aefceb678",
            "02ab9e68d55978174cc3cd7b175002102973a3a28955c033d1e1e7fbe37f08c8",
            "43d4ba1dbc9d4ac73417a8ed5485bcd314343d2840411eb654ab55a6ca26e8e3",
            "31b87ed495992a21509fe3c2b4501be0ae04b47810951743aa147a71119dbefb",
            "6fb064be3b0c67be3e351b4035c38fee6f42d2797b87c1a78b13015760939bf5",
            "32c8f52dff607635a3c3d324fd415c270ac3cce641a9c5711ca192d863cef886",
            "712a32517f7efcfef5e4d097fabf3072374a354e1878ad05b287b4b392a4ba68",
            "5077ea4ff2219cece8d0d3d18c51fdac326a5eaef4934b960bda12598b904273",
            "61ca77d59149d5948c94ac39fac0be0cfcd6b237021b4844ef07684647832023",
            "46d39a2fbe121eba80b2430b95bb28e66c1c0999dda68a81d76e978ed5511233",
            "0fef1ec614a2c5aee320c7cd9a7c59902ba04e97b3421716b78a299254723fd1",
            "3bf9a2632f7546e2e6d879ddf104fa1323b0c84a192090e32bb9fc17c59c1b30",
            "70aecdc2b11b46ca68799cb2782e23d601e109181f7dd8d762335cfcc80e9acb",
            "301204f5ed9dfea82142f3b575ec4a755700bba42ac1c98817a0bff67ee503d0",
            "5bca90afb2c8354ca33131f2acddab3cbb3f8b68718c729eeb977ea32702c6c1",
            "6b116041162022b3ded925d5c7187c9c140e6193d3ddf241ccd9c760ebf78f55",
            "6ddbf329a4506bba4d864590639fadabba005bffb46387f63afa6042c79d7450",
            "0b44b2ec95a379363b23023bf00fd38347ac764ffb706ab8be99bc8ec3fd4c0f",
            "6fbec9c662e7d37dad71162fcc7285fb44149d3e1a0c5ab1fbed43433c1f1e2f",
            "31a499dc077fd762a935582445f6bfc3f3beb14c3dd9844714541472fcef5d92",
            "47c05ea8a4c9f7585d16e994ee1c7151bb7473913d5d67424cf21e2bf997d1ce",
            "69b2c739a0af1616ec3f3145b40e847f12ef9d4d36f88ef05997982a8e6cf4c8",
            "045599d7cfbe59ddd5dd9e6d5455e4892d8c3d9ed937de67a32f537b74faac83",
            "4180ecb4a6d7a2c379cd362d11d1c81b27d098c4cf175557c68c9dfb6073e171",
            "6a9e2a7629c54cfc90442b97a7173dbdf02236ef57184c2cdc783f3aeaaaec25",
            "081eebae03b871b8547c3a4186aa429d743dab8afbda78e04be148e2c5400b06",
            "70cf891b57d7df67da6b3247dd0d31c17884bbfe547fb53893f83db4b8635d74",
            "4f83acee4f631dc30ad98ca4f2403c2af56601ad37cfe50e00afba45f33b3718",
            "3421455f8cda28666b09e1507aea62c1135137f17faeb3089e6b2b33e835622f",
            "02936d12fd34a1c8dd6112bd9f7bdcd68f6e22ebb86b1caad0970c1e7bd5c304",
            "6c5d23048131c8f509b2aba413a3cec711142cb5b46b3913c8855d6badffe12b",
            "39d6ee7ea4bc212e3aa9ef79083ca6b3896ac035b492b900834b19222045ec60",
            "72039a185a577483f54f3fad2c7ad121297dcee9785190a27ff1ef4bef1265b0",
            "4536a90acf9e343fcf48f161acb51b4b63d79267ad48d4711d84c065b7fee6f7",
            "268db31b30762b3264962d0147125c18149701c92083c3244a7a412b96de3e6c",
            "4099a33a009565a1a7180c2369d60a32b6ac86b2cee78cce406cf06ebfbc7d37",
            "2f152693330f62c1c4562551ae2dd293487beb8f1dbbc03c5c351ab4c6876483",
            "6a7f637e0c44acadaec2f175310656884556d070bf1156b0663b4de73983e142",
            "699e0d3cb2f63eea8295ce7a86e9f1cff901e9e1c442037c2c1b35962b62a538",
            "11d6ae790e3a80d32d3940f7fb689274f0ca3918009135d8bfe20567bc238161",
            "03925f6bb6e3bc8a20ffca4c4474e668c5dcdef27e28154e26e2e78b1f11d1c0",
            "0a935093b701ecf1ef625719557818b0cc80ea653d1d2a64bb9653d71b122e0c",
            "1bbb0ef6f9fac7e39cf154878296d8109ac12db357464bb716bc607704a8e5e5",
            "489b413a64c9c4764cecf15a8461125f75ae9e83a3efc0f24aabdcc1892c6b13",
            "19d49758d5b83b38ef9496725ffafb71bb1bd92f613d6e0a5cf9f86b3fabeeba",
            "04c9d29f650681f95a8c965619df087161c378f1c5ad30f655ca676a822f704d",
            "0f7bf1e46f87e22fdf3adeb685cba9a2c8bd718aa9ac958983b8fdf5af764cbf",
            "4b6d51f64330f22364f0426d8e87e8ce19352b9aefa0882e3cc229e73182aa3a",
            "50e14fa1e82e7d0d2b1967a996368824ce9f9402c0ba12cc452862fe972338e5",
            "6793c61c3a11403265cba801a46a7b896b2029de6455d6e085bc9ed50973c3b5",
            "5d978fa74d73f45750b5336c30ac748212eb2468927194df860b0c18ad3a1d72",
            "047808fb3f3ecf3cae77f91f87a16753779af6416a208ed49ce511ebe31fbbb7",
            "227c7463a365b0ed8a91862ffccb63bb2fba8d5b5f8082d31b08107b19346397",
            "361a0b9b999ba608a2ddb20255dc42dc10405e5fb4bef80fa8c54a08c590ce86",
            "03520b1da68b86701ada0c85b0c716a3871f27cd418b2cabfa629353cb8ac5ee",
            "11657fe0f1cf5bddcb22228e11ba2de19e56887f45c7204fbb8b0e0e3597f0fe",
            "5a28adf756b711daf394cca2ae24632f3d55b829f75673ba3394738fd3ccb5e4",
            "69cb76bed52b87856fc4b8cae25310b3ef80de6a96ca60b999ca82ea9bcb67f2",
            "19583a4fd4daaf4b94fc08e2c5f653404e1af5a803e91f3780325f041f857c2d",
            "12d354299b367fea7089e38db282a9ba257cb32ac7afe0481f81e8b21cbedf3c",
            "139aaeb1a235525a3e7297dc3391812f6fb31b8649ab3fb42f7569da1b694d02",
            "329239fa012a2c7931e575bb963cc21003f9760a4b3d0c1e757fa029b0c8ec9a",
            "12e168afc44477ff5483d54babe1021c26b90f812ba9cf5486a979117eb23c73",
            "09e9ca5756de544e672472b1da03dade66f9bf0d217cb3313dece380ca413ee5",
            "110116855dee47e4eac6e61c05f760a7239cacdf7b1578377dcb913787f6a1bd",
            "652cda305738a933c143446fa7b4785d7fed02876054a9fc05a6d0240e6d3314",
            "0499cbb1d794257e8b53b52e370e02f38fc88533658a37434faf904634ca551a",
            "0bd6cb1c4143f7ac22060ee9b1d469c948c384512473e4068b3af9637a707f5a",
            "1610b8e9138aee6dbf4fdadecc70f83da279050176eb62c164907774cc473cbb",
            "3102a5884d3dce8d94a8cf6d5ab2d3a4c76ec8b00f4554caa68c028aedf5970f",
            "3de2be346b539395b0c0de56a5ccca54a317f1b5c80107b0802af9a62276a4d8"
        };

        public static Banderwagon[] Generate()
        {
            int crsLength = _constants.Length;
            Banderwagon[] points = new Banderwagon[crsLength];

            for (int i = 0; i < crsLength; i++)
            {
                points[i] = new Banderwagon(_constants[i]);
            }

            return points;
        }
    }
}
