<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MediaKeywordFrequency.aspx.cs" Inherits="Pages_Charts_MediaKeywordFrequency" %>
<%@ Register TagPrefix="dotnet" Namespace="dotnetCHARTING" Assembly="dotnetCHARTING"%>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Media keyword tracking graph</title>
</head>
<body style="margin:0">
    <form id="form1" runat="server">
    <dotnet:Chart ID="Chart" runat="server"></dotnet:Chart>
    </form>
</body>
</html>