<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Payment.Login" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server" defaultbutton="btnSubmit" defaultfocus="txtPass">
    <table width="100%">
        <tr>
        <td>&nbsp;</td>
        </tr>
       <tr>
        <td>&nbsp;</td>
        </tr>
        <tr>
        <td>&nbsp;</td>
        </tr>
        <tr>
        <td align="center">
        <table width="30%" class="borderystyle" >
        <tr>
        <td align="center">
          <table>
            <tr>
            <td colspan="4" align="center"><img id="Img1" src="~/Images/egencia_logo_RGB.png" runat="server" 
                    alt="Egenica" width="400" /></td>
            </tr>
         
             <tr>
            <td colspan="4" align="center">
                        <asp:Label ID="msg" runat="server" Text=""></asp:Label>
                    </td>
            </tr>
            <tr>
            <td colspan="4">&nbsp;</td>
            </tr>
            </table>
            <table width="50%">
                <tr>
                    <td width="40%" align="right" >
                        <asp:Label ID="lblUser" runat="server" Text=" User Name :"  Font-Bold="true" ></asp:Label>&nbsp;&nbsp;
                    </td>
                    <td width="60%" align="left" >
                        <asp:TextBox ID="txtUser" runat="server" MaxLength="25" 
                             TabIndex="1" CssClass="logintextbox" ></asp:TextBox>&nbsp; <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*"
                            ControlToValidate="txtUser" ForeColor="Red"></asp:RequiredFieldValidator>
                      
                    </td>
                   
                  
                </tr>
                <tr><td colspan="3">&nbsp;</td></tr>
                <tr>
                    <td align="right" >
                        <asp:Label ID="lblPassword" runat="server" Text=" Password :" Font-Bold="true" ></asp:Label>&nbsp;&nbsp;
                    </td>
                    <td align="left">
                        <asp:TextBox ID="txtPass" runat="server" TextMode="Password" 
                            MaxLength="25"  TabIndex="2" CssClass="logintextbox"  ></asp:TextBox> &nbsp;
                           <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="*"
                            ControlToValidate="txtPass" ForeColor="Red"></asp:RequiredFieldValidator>
                   </td>
                        
                   
                  
                </tr>
                <tr>
                <td colspan="3">&nbsp;</td>
                </tr>
                <tr>
                <td align="right">
                    <asp:Button ID="btnSubmit" runat="server" Text="Login" 
                        OnClick="btnSubmit_Click" CssClass="button1" TabIndex="3" /> </td>
                <td align="left">
                    <asp:Button ID="btnReset" runat="server" Text="Reset" 
                        OnClick="btnReset_Click" CssClass="button1"  TabIndex="4" /></td>
                <td>&nbsp;</td>
                </tr>
                <tr>
                <td colspan="3"> &nbsp;</td>
                </tr>
            </table>
  
        
            
           
        
                <%--<asp:TextBox ID="txtDate2" runat="server" ReadOnly=true></asp:TextBox>--%>
         
        </td>
        
        </tr></table>
        </td>
        </tr>
        </table>
</asp:Content>
