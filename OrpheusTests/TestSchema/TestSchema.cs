using OrpheusCore;
using OrpheusInterfaces.Core;
using OrpheusInterfaces.Schema;
using OrpheusTestModels;
using System;
using System.Collections.Generic;

namespace OrpheusTests
{
    public static class TestSchemaConstants
    {
        public static readonly Guid AdminUserProfileId = new Guid("C2DDC7EC-54C6-4440-A775-3879095B8CE2");
        public static readonly Guid MemberProfileId = new Guid("C3706901-F844-4F66-85EE-8D9ECD80B31C");
        public static readonly Guid AdminUserGroupId = new Guid("CC202F19-336E-4812-8535-1E98FB8422B0");
        public static readonly Guid MemberUserGroupId = new Guid("169D1B45-DA30-4342-8283-8556BBE59FE0");
        public static readonly Guid AdminUserId = new Guid("191A3228-861B-4E57-BC03-9304AD1C099C");
    }
    public class TestSchema
    {
        private ISchema schema;

        private void createUserProfiles()
        {
            var userProfiles = this.schema.AddSchemaTable(typeof(TestModelUserProfile));
            var userProfilesData = new List<TestModelUserProfile>();
            userProfilesData.Add(new TestModelUserProfile() { UserProfileId = TestSchemaConstants.AdminUserProfileId, Description = "Admin profile", CanSearch = true, CanTalkToOthers = true });
            userProfilesData.Add(new TestModelUserProfile() { UserProfileId = TestSchemaConstants.MemberProfileId, Description = "Member profile", CanSearch = true, CanTalkToOthers = false });
            userProfiles.SetData<TestModelUserProfile>(userProfilesData);
        }

        private void createUserGroups()
        {
            var userGroups = this.schema.AddSchemaTable(typeof(TestModelUserGroup));
            var userGroupsData = new List<TestModelUserGroup>();
            userGroupsData.Add(new TestModelUserGroup() { UserGroupId = TestSchemaConstants.AdminUserGroupId, Description = "Administrators" });
            userGroupsData.Add(new TestModelUserGroup() { UserGroupId = TestSchemaConstants.MemberUserGroupId, Description = "Members" });
            userGroups.SetData<TestModelUserGroup>(userGroupsData);
        }

        private void createUsers()
        {
            var users = this.schema.AddSchemaTable(typeof(TestModelUser));

            users.AddDependency(typeof(TestModelUserProfile));
            users.AddDependency(typeof(TestModelUserGroup));

            var saltHash = "salt";
            try
            {

                var usersData = new List<TestModelUser>();
                usersData.Add(new TestModelUser()
                {
                    UserId = TestSchemaConstants.AdminUserId,
                    Active = 1,
                    UserName = "Administrator",
                    Email = "webadmin@thelanguageexchange.com",
                    PasswordHash = "passhash",
                    PasswordSalt = saltHash,
                    UserProfileId = TestSchemaConstants.AdminUserProfileId,
                    UserGroupId = TestSchemaConstants.AdminUserGroupId
                });
                users.SetData<TestModelUser>(usersData);
            }
            finally
            {
            }
        }

        private void createLanguages()
        {
            var languages = this.schema.AddSchemaTable(typeof(TestModelLanguage));
            var languagesData = new List<TestModelLanguage>();
            languagesData.Add(new TestModelLanguage() { LanguageId = Guid.NewGuid(), Code = "EN", Name = "English" });
            languagesData.Add(new TestModelLanguage() { LanguageId = Guid.NewGuid(), Code = "EL", Name = "Greek" });
            languagesData.Add(new TestModelLanguage() { LanguageId = Guid.NewGuid(), Code = "IT", Name = "Italian" });
            languagesData.Add(new TestModelLanguage() { LanguageId = Guid.NewGuid(), Code = "ES", Name = "Spanish" });
            languagesData.Add(new TestModelLanguage() { LanguageId = Guid.NewGuid(), Code = "PT", Name = "Portuguese" });
            languagesData.Add(new TestModelLanguage() { LanguageId = Guid.NewGuid(), Code = "FR", Name = "French" });
            languagesData.Add(new TestModelLanguage() { LanguageId = Guid.NewGuid(), Code = "DE", Name = "German" });
            languages.SetData<TestModelLanguage>(languagesData);
        }

        private void createCountries()
        {
            var countries = this.schema.AddSchemaTable(typeof(TestModelCountry));
            //this.countries = OrpheusIocContainer.Resolve<ISchemaTable>();
            //this.countries.AddField("CountryId","UNIQUEIDENTIFIER", false, " NEWID()" );
            //this.countries.AddField("Code","NVARCHAR", true, null, "30" );
            //this.countries.AddField("Name",  "NVARCHAR", true,null,  "50" );
            //this.countries.SQLName = "TEST_MODEL_COUNTRIES";

            //this.schema.AddSchemaObject(this.countries);

            var countriesData = new List<TestModelCountry>();

            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "AL", Name = "Albania" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "DZ", Name = "Algeria" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "DS", Name = "American Samoa" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "AD", Name = "Andorra" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "AO", Name = "Angola" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "AI", Name = "Anguilla" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "AQ", Name = "Antarctica" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "AG", Name = "Antigua and/or Barbuda" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "AR", Name = "Argentina" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "AM", Name = "Armenia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "AW", Name = "Aruba" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "AU", Name = "Australia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "AT", Name = "Austria" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "AZ", Name = "Azerbaijan" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "BS", Name = "Bahamas" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "BH", Name = "Bahrain" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "BD", Name = "Bangladesh" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "BB", Name = "Barbados" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "BY", Name = "Belarus" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "BE", Name = "Belgium" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "BZ", Name = "Belize" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "BJ", Name = "Benin" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "BM", Name = "Bermuda" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "BT", Name = "Bhutan" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "BO", Name = "Bolivia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "BA", Name = "Bosnia and Herzegovina" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "BW", Name = "Botswana" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "BV", Name = "Bouvet Island" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "BR", Name = "Brazil" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "IO", Name = "British lndian Ocean Territory" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "BN", Name = "Brunei Darussalam" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "BG", Name = "Bulgaria" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "BF", Name = "Burkina Faso" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "BI", Name = "Burundi" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "KH", Name = "Cambodia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "CM", Name = "Cameroon" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "CA", Name = "Canada" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "CV", Name = "Cape Verde" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "KY", Name = "Cayman Islands" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "CF", Name = "Central African Republic" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "TD", Name = "Chad" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "CL", Name = "Chile" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "CN", Name = "China" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "CX", Name = "Christmas Island" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "CC", Name = "Cocos (Keeling) Islands" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "CO", Name = "Colombia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "KM", Name = "Comoros" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "CG", Name = "Congo" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "CK", Name = "Cook Islands" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "CR", Name = "Costa Rica" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "HR", Name = "Croatia (Hrvatska)" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "CU", Name = "Cuba" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "CY", Name = "Cyprus" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "CZ", Name = "Czech Republic" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "DK", Name = "Denmark" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "DJ", Name = "Djibouti" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "DM", Name = "Dominica" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "DO", Name = "Dominican Republic" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "TP", Name = "East Timor" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "EC", Name = "Ecuador" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "EG", Name = "Egypt" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "SV", Name = "El Salvador" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "GQ", Name = "Equatorial Guinea" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "ER", Name = "Eritrea" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "EE", Name = "Estonia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "ET", Name = "Ethiopia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "FK", Name = "Falkland Islands (Malvinas)" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "FO", Name = "Faroe Islands" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "FJ", Name = "Fiji" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "FI", Name = "Finland" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "FR", Name = "France" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "FX", Name = "France, Metropolitan" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "GF", Name = "French Guiana" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "PF", Name = "French Polynesia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "TF", Name = "French Southern Territories" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "GA", Name = "Gabon" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "GM", Name = "Gambia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "GE", Name = "Georgia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "DE", Name = "Germany" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "GH", Name = "Ghana" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "GI", Name = "Gibraltar" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "GR", Name = "Greece" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "GL", Name = "Greenland" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "GD", Name = "Grenada" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "GP", Name = "Guadeloupe" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "GU", Name = "Guam" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "GT", Name = "Guatemala" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "GN", Name = "Guinea" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "GW", Name = "Guinea-Bissau" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "GY", Name = "Guyana" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "HT", Name = "Haiti" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "HM", Name = "Heard and Mc Donald Islands" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "HN", Name = "Honduras" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "HK", Name = "Hong Kong" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "HU", Name = "Hungary" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "IS", Name = "Iceland" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "IN", Name = "India" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "ID", Name = "Indonesia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "IR", Name = "Iran (Islamic Republic of)" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "IQ", Name = "Iraq" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "IE", Name = "Ireland" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "IL", Name = "Israel" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "IT", Name = "Italy" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "CI", Name = "Ivory Coast" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "JM", Name = "Jamaica" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "JP", Name = "Japan" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "JO", Name = "Jordan" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "KZ", Name = "Kazakhstan" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "KE", Name = "Kenya" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "KI", Name = "Kiribati" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "KP", Name = "Korea, Democratic People's Republic of" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "KR", Name = "Korea, Republic of" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "XK", Name = "Kosovo" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "KW", Name = "Kuwait" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "KG", Name = "Kyrgyzstan" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "LA", Name = "Lao People's Democratic Republic" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "LV", Name = "Latvia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "LB", Name = "Lebanon" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "LS", Name = "Lesotho" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "LR", Name = "Liberia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "LY", Name = "Libyan Arab Jamahiriya" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "LI", Name = "Liechtenstein" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "LT", Name = "Lithuania" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "LU", Name = "Luxembourg" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "MO", Name = "Macau" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "MK", Name = "Macedonia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "MG", Name = "Madagascar" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "MW", Name = "Malawi" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "MY", Name = "Malaysia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "MV", Name = "Maldives" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "ML", Name = "Mali" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "MT", Name = "Malta" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "MH", Name = "Marshall Islands" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "MQ", Name = "Martinique" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "MR", Name = "Mauritania" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "MU", Name = "Mauritius" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "TY", Name = "Mayotte" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "MX", Name = "Mexico" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "FM", Name = "Micronesia, Federated States of" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "MD", Name = "Moldova, Republic of" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "MC", Name = "Monaco" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "MN", Name = "Mongolia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "ME", Name = "Montenegro" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "MS", Name = "Montserrat" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "MA", Name = "Morocco" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "MZ", Name = "Mozambique" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "MM", Name = "Myanmar" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "NA", Name = "Namibia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "NR", Name = "Nauru" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "NP", Name = "Nepal" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "NL", Name = "Netherlands" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "AN", Name = "Netherlands Antilles" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "NC", Name = "New Caledonia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "NZ", Name = "New Zealand" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "NI", Name = "Nicaragua" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "NE", Name = "Niger" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "NG", Name = "Nigeria" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "NU", Name = "Niue" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "NF", Name = "Norfork Island" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "MP", Name = "Northern Mariana Islands" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "NO", Name = "Norway" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "OM", Name = "Oman" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "PK", Name = "Pakistan" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "PW", Name = "Palau" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "PA", Name = "Panama" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "PG", Name = "Papua New Guinea" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "PY", Name = "Paraguay" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "PE", Name = "Peru" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "PH", Name = "Philippines" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "PN", Name = "Pitcairn" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "PL", Name = "Poland" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "PT", Name = "Portugal" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "PR", Name = "Puerto Rico" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "QA", Name = "Qatar" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "RE", Name = "Reunion" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "RO", Name = "Romania" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "RU", Name = "Russian Federation" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "RW", Name = "Rwanda" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "KN", Name = "Saint Kitts and Nevis" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "LC", Name = "Saint Lucia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "VC", Name = "Saint Vincent and the Grenadines" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "WS", Name = "Samoa" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "SM", Name = "San Marino" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "ST", Name = "Sao Tome and Principe" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "SA", Name = "Saudi Arabia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "SN", Name = "Senegal" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "RS", Name = "Serbia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "SC", Name = "Seychelles" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "SL", Name = "Sierra Leone" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "SG", Name = "Singapore" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "SK", Name = "Slovakia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "SI", Name = "Slovenia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "SB", Name = "Solomon Islands" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "SO", Name = "Somalia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "ZA", Name = "South Africa" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "GS", Name = "South Georgia South Sandwich Islands" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "ES", Name = "Spain" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "LK", Name = "Sri Lanka" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "SH", Name = "St. Helena" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "PM", Name = "St. Pierre and Miquelon" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "SD", Name = "Sudan" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "SR", Name = "Suriname" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "SJ", Name = "Svalbarn and Jan Mayen Islands" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "SZ", Name = "Swaziland" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "SE", Name = "Sweden" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "CH", Name = "Switzerland" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "SY", Name = "Syrian Arab Republic" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "TW", Name = "Taiwan" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "TJ", Name = "Tajikistan" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "TZ", Name = "Tanzania, United Republic of" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "TH", Name = "Thailand" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "TG", Name = "Togo" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "TK", Name = "Tokelau" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "TO", Name = "Tonga" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "TT", Name = "Trinidad and Tobago" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "TN", Name = "Tunisia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "TR", Name = "Turkey" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "TM", Name = "Turkmenistan" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "TC", Name = "Turks and Caicos Islands" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "TV", Name = "Tuvalu" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "UG", Name = "Uganda" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "UA", Name = "Ukraine" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "AE", Name = "United Arab Emirates" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "GB", Name = "United Kingdom" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "UM", Name = "United States minor outlying islands" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "UY", Name = "Uruguay" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "UZ", Name = "Uzbekistan" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "US", Name = "United States" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "VU", Name = "Vanuatu" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "VA", Name = "Vatican City State" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "VE", Name = "Venezuela" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "VN", Name = "Vietnam" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "VG", Name = "Virgin Islands (British)" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "VI", Name = "Virgin Islands (U.S.)" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "WF", Name = "Wallis and Futuna Islands" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "EH", Name = "Western Sahara" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "YE", Name = "Yemen" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "YU", Name = "Yugoslavia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "ZR", Name = "Zaire" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "ZM", Name = "Zambia" });
            countriesData.Add(new TestModelCountry() { CountryId = Guid.NewGuid(), Code = "ZW", Name = "Zimbabwe" });
            countries.SetData<TestModelCountry>(countriesData);
        }

        private void createMembers()
        {
            var members = this.schema.AddSchemaTable<TestModelMember>();

            members.AddDependency<TestModelCountry>();
            members.AddDependency<TestModelUser>();

            var memberData = new List<TestModelMember>();
            memberData.Add(new TestModelMember()
            {
                MemberId = Guid.NewGuid(),
                FirstName = "Administrator",
                LastName = "Administrator",
                UserId = TestSchemaConstants.AdminUserId,
                IsProfilePublic = false,
                Gender = 0,
                ProfilePicture = new byte[0]
            });
            members.SetData<TestModelMember>(memberData);
        }

        private void createContactTypes()
        {
            var contactTypes = this.schema.AddSchemaTable(typeof(TestModelContactType));

            var contactTypeData = new List<TestModelContactType>() {
                new TestModelContactType() { ContactTypeId= Guid.NewGuid(), Name ="Skype" },
                new TestModelContactType() { ContactTypeId= Guid.NewGuid(), Name ="Facebook" },
                new TestModelContactType() { ContactTypeId= Guid.NewGuid(), Name ="Email" }
            };

            contactTypes.SetData<TestModelContactType>(contactTypeData);
        }

        private void createMemberLanguages()
        {
            var memberLanguagesToLearn = this.schema.AddSchemaTable(typeof(TestModelMemberLanguage));
            memberLanguagesToLearn.AddDependency(typeof(TestModelMember));
            memberLanguagesToLearn.AddDependency(typeof(TestModelLanguage));
        }

        private void createMemberLanguagesToLearn()
        {
            var memberLanguagesToLearn = this.schema.AddSchemaTable(typeof(TestModelMemberLanguageToLearn));
            memberLanguagesToLearn.AddDependency(typeof(TestModelMember));
            memberLanguagesToLearn.AddDependency(typeof(TestModelLanguage));
        }

        private void createMemberContactTypes()
        {
            var memberContactTypes = this.schema.AddSchemaTable(typeof(TestModelMemberContactType));
            memberContactTypes.AddDependency(typeof(TestModelContactType));
            memberContactTypes.AddDependency(typeof(TestModelMember));

        }

        private void createOrdersSchema()
        {
            var transactors = this.schema.AddSchemaTable(typeof(TestModelTransactor));



            var items = this.schema.AddSchemaTable(typeof(TestModelItem));


            var orders = this.schema.AddSchemaTable(typeof(TestModelOrder));
            orders.AddDependency(typeof(TestModelTransactor));

            var orderLines = this.schema.AddSchemaTable(typeof(TestModelOrderLine));
            orderLines.AddDependency(typeof(TestModelOrder));
            orderLines.AddDependency(typeof(TestModelItem));
        }

        private string getTypeSQLName<T>()
        {
            return this.schema.Name == null ? typeof(T).Name : $"{this.schema.Name}.{typeof(T).Name}";
        }

        private void createMasterDetailTenLevelSchema()
        {
            this.schema.AddSchemaTable<TestMasterModel>();
            this.schema.AddSchemaTable<TestDetailModelLevel1>().AddDependency<TestMasterModel>();
            this.schema.AddSchemaTable<TestDetailModelLevel2>().AddDependency<TestDetailModelLevel1>();
            this.schema.AddSchemaTable<TestDetailModelLevel3>().AddDependency<TestDetailModelLevel2>();
            this.schema.AddSchemaTable<TestDetailModelLevel4>().AddDependency<TestDetailModelLevel3>();
            this.schema.AddSchemaTable<TestDetailModelLevel5>().AddDependency<TestDetailModelLevel4>();
            this.schema.AddSchemaTable<TestDetailModelLevel6>().AddDependency<TestDetailModelLevel5>();
            this.schema.AddSchemaTable<TestDetailModelLevel7>().AddDependency<TestDetailModelLevel6>();
            this.schema.AddSchemaTable<TestDetailModelLevel8>().AddDependency<TestDetailModelLevel7>();
            this.schema.AddSchemaTable<TestDetailModelLevel9>().AddDependency<TestDetailModelLevel8>();
            this.schema.AddSchemaTable<TestDetailModelLevel10>().AddDependency<TestDetailModelLevel9>();
        }

        private void createBinarySchema()
        {
            this.schema.AddSchemaTable(typeof(TestBinaryDataModel));
        }

        private void createSchema()
        {

            this.createUserProfiles();
            this.createUserGroups();
            this.createUsers();
            this.createLanguages();
            this.createCountries();
            this.createMembers();
            this.createContactTypes();
            this.createMemberLanguages();
            this.createMemberLanguagesToLearn();
            this.createMemberContactTypes();
            this.createOrdersSchema();
            this.createMasterDetailTenLevelSchema();
            this.createBinarySchema();
        }

        public List<ISchemaObject> SchemaObjects { get { return this.schema.SchemaObjects; } }

        public void Execute() { this.schema.Execute(); }

        public void Drop() { this.schema.Drop(); }

        public void SaveToFile(string fileName) { this.schema.SaveToFile(fileName); }
        public void LoadFromFile(string fileName) { this.schema.LoadFromFile(fileName); }

        public ISchema Schema { get { return this.schema; } }
        public TestSchema(IOrpheusDatabase db, string description, double version, Guid id, string name = null)
        {
            this.schema = ServiceManager.Resolve<ISchema>(new object[] { db, description, version, id, name });
            this.createSchema();
        }
    }
}
