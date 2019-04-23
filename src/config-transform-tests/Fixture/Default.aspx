<%@ Page Language="C#"  AutoEventWireup="true" %>
<%@ Import Namespace="FluentAssertions.Common" %>
<html>
<body>
<pre>
    AppSettings["MyKey"] = <%= ConfigurationManager.AppSettings["MyKey"] %>
    ConnectionStrings["MyConnectionString"] = <%= ConfigurationManager.ConnectionStrings["MyConnectionString"] %>
</pre>
</body>
</html>
