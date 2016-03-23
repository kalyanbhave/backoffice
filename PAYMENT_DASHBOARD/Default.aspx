<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="Payment._Default" %>


<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">
</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js"></script>
<%--<script type="text/javascript">
    function ShowProgress() {
        debugger;
        setTimeout(function () {
            var modal = $('<div />');
            modal.addClass("modal");
            $('body').append(modal);
            var loading = $(".loading");
            loading.show();
            var top = Math.max($(window).height() / 2 - loading[0].offsetHeight / 2, 0);
            var left = Math.max($(window).width() / 2 - loading[0].offsetWidth / 2, 0);
            loading.css({ top: top, left: left });
        }, 200);
    }
    $('form').live("submit", function () {
        ShowProgress();
    });
</script>--%>
   
    <div>

   <%-- <asp:CheckBox ID="chkBrder" runat="server"
         Text="Put Border Around Cells" />
    <br /><br />--%>
    <asp:Button ID="cmdCreate" OnClick="cmdCreate_Click" runat="server"
     Text="Refresh" />
    <br /><br />
   <asp:Table ID="tb2" runat="server" CellPadding="3" CellSpacing="0" Width="100%" BorderWidth="1px" GridLines="Both">
   <asp:TableHeaderRow runat="server"> 
   <asp:TableCell ID="Cell1" Text="" runat="server" Width="5%" >
   </asp:TableCell>
   <asp:TableCell id="Cell2" Text="Classic" align="center" runat="server" Width="74%" Font-Bold="true" style="font-size:medium;" BackColor="AntiqueWhite">
   </asp:TableCell>
    <asp:TableCell ID="Cell3"  align="center" Text="VIA" style="font-size:medium;" runat="server" Width="21%" Font-Bold="true" BackColor="AntiqueWhite">
   </asp:TableCell>
   </asp:TableHeaderRow>
   </asp:Table>
    <asp:Table ID="tbl" runat="server"  CellPadding="3" CellSpacing="0" Width="100%" BorderWidth="1px" GridLines="Both"></asp:Table>

   <br />
   <br />
                                
  </div>

  <%--<div class="loading" align="center">
    Loading. Please wait.<br />
    <br />
    <img src="Images/loader.gif" alt="" />
</div>--%>
</asp:Content>
