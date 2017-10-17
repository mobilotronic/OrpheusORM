using OrpheusAttributes;
using System;
using System.Dynamic;

namespace OrpheusTestModels
{
    public enum TestModelTransactorType
    {
        ttCustomer,
        ttSupplier
    }
    public class TestModelTransactor
    {
        [PrimaryKey]
        public Guid TransactorId { get; set; }

        [Length(30)]
        public string Code { get; set; }

        [Length(120)]
        public string Description { get; set; }

        [Length(120)]
        public string Address { get; set; }

        [Length(250)]
        public string Email { get; set; }

        public TestModelTransactorType Type { get; set; }
    }

    public class TestModelItem
    {
        [PrimaryKey(false)]
        public Guid ItemId { get; set; }

        [Length(30)]
        public string Code { get; set; }

        [Length(120)]
        public string Description { get; set; }

        [DefaultValue(0)]
        public double Price { get; set; }
    }

    public class TestModelOrder
    {
        [PrimaryKey]
        public Guid OrderId { get; set; }

        [ForeignKey("TestModelTransactor", "TransactorId")]
        public Guid TransactorId { get; set; }

        public DateTime OrderDateTime { get; set; }
    }

    public class TestModelOrderLine
    {
        [PrimaryKey]
        public Guid OrderLineId { get; set; }

        [ForeignKey("TestModelOrder", "OrderId")]
        public Guid OrderId { get; set; }

        [ForeignKey("TestModelItem", "ItemId")]
        public Guid ItemId { get; set; }

        [DefaultValue(0)]
        public double Quantity { get; set; }

        public double Price { get; set; }

        public double TotalPrice { get; set; }
    }

    public class TestModelLanguage
    {
        [PrimaryKey]
        public Guid LanguageId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class TestModelUserProfile
    {
        [PrimaryKey]
        public Guid UserProfileId { get; set; }
        public string Description { get; set; }
        [DefaultValue(1)]
        public bool CanSearch { get; set; }
        [DefaultValue(0)]
        public bool CanTalkToOthers { get; set; }
    }

    public class TestModelUserGroup
    {
        [PrimaryKey]
        public Guid UserGroupId { get; set; }
        public string Description { get; set; }
    }

    public class TestModelUser
    {
        [PrimaryKey]
        public Guid UserId { get; set; }
        [UniqueKey]
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
        public string Email { get; set; }
        public int Active { get; set; }
        [ForeignKey("TestModelUserProfile", "UserProfileId")]
        public Guid UserProfileId { get; set; }
        [ForeignKey("TestModelUserGroup", "UserGroupId")]
        public Guid UserGroupId { get; set; }
    }

    public class TestModelMember
    {
        [PrimaryKey]
        public Guid MemberId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SkypeName { get; set; }
        public string FacebookName { get; set; }
        public string ProfileSummary { get; set; }
        [DefaultValue(null)]
        public byte[] ProfilePicture { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public int Gender { get; set; }
        public bool IsProfilePublic { get; set; }
        public string NickName { get; set; }

        [ForeignKey("TestModelCountry", "CountryId")]
        public Guid? CountryId { get; set; }

        [ForeignKey("TestModelUser", "UserId")]
        public Guid? UserId { get; set; }
    }

    [PrimaryCompositeKey(new string[] { "MemberId", "LanguageId" })]
    public class TestModelMemberLanguage
    {
        [ForeignKey("TestModelMember", "MemberId")]
        public Guid MemberId { get; set; }

        [ForeignKey("TestModelLanguage", "LanguageId")]
        public Guid LanguageId { get; set; }
    }

    [PrimaryCompositeKey(new string[] { "MemberId", "LanguageId" })]
    public class TestModelMemberLanguageToLearn
    {
        [ForeignKey("TestModelMember", "MemberId")]
        public Guid MemberId { get; set; }

        [ForeignKey("TestModelLanguage", "LanguageId")]
        public Guid LanguageId { get; set; }
    }

    public class TestModelCountry
    {
        [PrimaryKey]
        public Guid CountryId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class TestModelContactType
    {
        [PrimaryKey]
        public Guid ContactTypeId { get; set; }
        public string Name { get; set; }
    }

    [PrimaryCompositeKey(new string[] { "MemberId","ContactTypeId" })]
    public class TestModelMemberContactType
    {
        public Guid MemberId { get; set; }
        public Guid ContactTypeId { get; set; }
    }

    #region Master-Detail 10 levels models

    public class TestMasterModel
    {
        [PrimaryKey]
        public Guid Key { get; set; }
    }

    public class TestDetailModelLevel1
    {
        [PrimaryKey]
        public Guid Key { get; set; }
        [ForeignKey(typeof(TestMasterModel), "Key")]
        public Guid MasterKey { get; set; }
    }

    public class TestDetailModelLevel2
    {
        [PrimaryKey]
        public Guid Key { get; set; }
        [ForeignKey(typeof(TestDetailModelLevel1), "Key")]
        public Guid MasterKey { get; set; }
    }

    public class TestDetailModelLevel3
    {
        [PrimaryKey]
        public Guid Key { get; set; }
        [ForeignKey(typeof(TestDetailModelLevel2), "Key")]
        public Guid MasterKey { get; set; }
    }

    public class TestDetailModelLevel4
    {
        [PrimaryKey]
        public Guid Key { get; set; }
        [ForeignKey(typeof(TestDetailModelLevel3), "Key")]
        public Guid MasterKey { get; set; }
    }

    public class TestDetailModelLevel5
    {
        [PrimaryKey]
        public Guid Key { get; set; }
        [ForeignKey(typeof(TestDetailModelLevel4), "Key")]
        public Guid MasterKey { get; set; }
    }

    public class TestDetailModelLevel6
    {
        [PrimaryKey]
        public Guid Key { get; set; }
        [ForeignKey(typeof(TestDetailModelLevel5), "Key")]
        public Guid MasterKey { get; set; }
    }

    public class TestDetailModelLevel7
    {
        [PrimaryKey]
        public Guid Key { get; set; }
        [ForeignKey(typeof(TestDetailModelLevel6), "Key")]
        public Guid MasterKey { get; set; }
    }

    public class TestDetailModelLevel8
    {
        [PrimaryKey]
        public Guid Key { get; set; }
        [ForeignKey(typeof(TestDetailModelLevel7), "Key")]
        public Guid MasterKey { get; set; }
    }

    public class TestDetailModelLevel9
    {
        [PrimaryKey]
        public Guid Key { get; set; }
        [ForeignKey(typeof(TestDetailModelLevel8), "Key")]
        public Guid MasterKey { get; set; }
    }

    public class TestDetailModelLevel10
    {
        [PrimaryKey]
        public Guid Key { get; set; }
        [ForeignKey(typeof(TestDetailModelLevel9), "Key")]
        public Guid MasterKey { get; set; }
    }

    public class TestBinaryDataModel
    {
        [PrimaryKey]
        public Guid Key { get; set; }

        [RequiredField]
        public byte[] Image { get; set; }
    }
    #endregion

    #region dynamic models
    public class TestDynamicModel1
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Index { get; set; }
    }

    public class TestDynamicModel2
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Description { get; set; }
    }

    public class TestDynamicModel3
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Description { get; set; }
        public short Price { get; set; }
        [Length(1)]
        public string Index { get; set; }
    }
    #endregion
}
