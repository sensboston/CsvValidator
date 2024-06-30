namespace CsvValidator
{
    public static class DefaultRules
    {
        public const string Xml = @"
<Rules>
   <Rule>
       <Name>Market Number</Name>
       <RegEx>^\d+$</RegEx>
       <IsUnique>false</IsUnique>
       <AllowEmpty>false</AllowEmpty>
   </Rule>
   <Rule>
       <Name>User ID</Name>
       <RegEx>^\d+\d+[a-zA-Z]{2}$</RegEx>
       <IsUnique>true</IsUnique>
       <AllowEmpty>false</AllowEmpty>
   </Rule>
   <Rule>
       <Name>Registered/Active/Deactive/BadEmail/Unsubscribed</Name>
       <RegEx>^(Registered|Active|Deactive|BadEmail|Unsubscribed)$</RegEx>
       <IsUnique>false</IsUnique>
       <AllowEmpty>false</AllowEmpty>
   </Rule>
   <Rule>
       <Name>Email</Name>
       <RegEx>^$|^[^@\s]+@[^@\s]+\.[^@\s]+$</RegEx>
       <IsUnique>true</IsUnique>
       <AllowEmpty>true</AllowEmpty>
   </Rule>
   <Rule>
       <Name>Phone Number</Name>
       <RegEx>^$|^\d{3}-\d{3}-\d{4}$|^\(\d{3}\) \d{3}-\d{4}$</RegEx>
       <IsUnique>false</IsUnique>
       <AllowEmpty>true</AllowEmpty>
   </Rule>
   <Rule>
       <Name>Street Address</Name>
       <RegEx>^.*$</RegEx>
       <IsUnique>false</IsUnique>
       <AllowEmpty>true</AllowEmpty>
   </Rule>
   <Rule>
       <Name>Zip</Name>
       <RegEx>^\d{5}$</RegEx>
       <IsUnique>false</IsUnique>
       <AllowEmpty>false</AllowEmpty>
   </Rule>
   <Rule>
       <Name>Website</Name>
       <RegEx>^$|^(http|https):\/\/[^ ""\s]+$|^[^ ""\s]+\.[^ ""\s]+$</RegEx>
       <IsUnique>false</IsUnique>
       <AllowEmpty>true</AllowEmpty>
   </Rule>
</Rules>";
    }
}
