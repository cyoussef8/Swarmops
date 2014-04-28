﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Master-v5.master" AutoEventWireup="true" CodeFile="ResyncExternalAccount.aspx.cs" Inherits="Swarmops.Frontend.Pages.v5.Ledgers.ResyncExternalAccount" %>
<%@ Register src="~/Controls/v5/UI/ExternalScripts.ascx" tagname="ExternalScripts" tagprefix="Swarmops5" %>
<%@ Register src="~/Controls/v5/Base/FileUpload.ascx" tagname="FileUpload" tagprefix="Swarmops5" %>


<asp:Content ID="Content1" ContentPlaceHolderID="PlaceHolderHead" Runat="Server">
    <Swarmops5:ExternalScripts ID="ExternalScripts1" Package="easyui" Control="tree" runat="server" />
    <!-- The Iframe Transport is required for browsers without support for XHR file uploads -->
    <script src="/Scripts/jquery.fileupload/jquery.iframe-transport.js" type="text/javascript" language="javascript"></script>
    <!-- The basic File Upload plugin -->
    <script src="/Scripts/jquery.fileupload/jquery.fileupload.js" type="text/javascript" language="javascript"></script>
	<link rel="stylesheet" type="text/css" href="/Style/v5-easyui-elements.css">

	<script type="text/javascript">

	    $(document).ready(function () {

	        $('#tableResyncPreview').treegrid(
	        {
	            /*
	            onBeforeExpand: function (foo) {
	                $('span.profitlossdata-collapsed-' + foo.id).fadeOut('fast', function () {
	                    $('span.profitlossdata-expanded-' + foo.id).fadeIn('slow');
	                });
	            },

	            onBeforeCollapse: function (foo) {
	                $('span.profitlossdata-expanded-' + foo.id).fadeOut('fast', function () {
	                    $('span.profitlossdata-collapsed-' + foo.id).fadeIn('slow');
	                });
	            },
                
	            onLoadSuccess: function () {
	                $('div.datagrid').css('opacity', 1);
	                $('#imageLoadIndicator').hide();
	                $('span.loadingHeader').hide();

	                var selectedYear = $('#<%=DropAccounts.ClientID %>').val();

	                $('div#linkDownloadReport').attr("onclick", "document.location='Csv-BalanceData.aspx?Year=" + selectedYear + "';");
	                $('#spanDownloadText').text('<%=Resources.Pages.Ledgers.BalanceSheet_DownloadFileName %>' + selectedYear + "-<%=DateTime.Today.ToString("yyyyMMdd") %>.csv");
                    $('#headerStartYear').text('<%=Resources.Pages.Ledgers.BalanceSheet_StartYear %>'.replace('XXXX',selectedYear));
              
                    if (selectedYear == currentYear) 
                    {
                        $('span.previousYearsHeader').hide();
                        $('span.currentYearHeader').show();
                    }
                    else
                    {
                        $('#previousYtd').text('<%=Resources.Pages.Ledgers.BalanceSheet_EndYear %>'.replace('XXXX', selectedYear));

                        $('span.currentYearHeader').hide();
                        $('span.previousYearsHeader').show();
                    }
	                
                    $('span.commonHeader').show();
	            }*/
	        });

	        $('#<%=DropAccounts.ClientID %>').change(function () {
	            var selectedAccountId = $('#<%=DropAccounts.ClientID %>').val();
	            
                if (selectedAccountId != 0) 
                {
                    $('#<%=this.UploadFile.ClientID %>_ButtonUploadVisible').fadeIn();
                } else {
                    $('#<%=this.UploadFile.ClientID %>_ButtonUploadVisible').fadeOut();
                }

	            $('#tableProfitLoss').treegrid({ url: 'Json-BalanceSheetData.aspx?Year=' + selectedYear });
        	    $('#imageLoadIndicator').show();
	            $('div.datagrid').css('opacity', 0.4);

	            $('#tableProfitLoss').treegid('reload');
	        });

	    });

        function uploadCompletedCallback() {
            $('#DivStepUpload').slideUp().fadeOut();
            $('#DivStepProcessing').fadeIn();
            $('#DivProgressProcessing').progressbar({ value: 0, max: 100 });
            
	        $.ajax({
	            type: "POST",
	            url: "ResyncExternalAccount.aspx/InitializeProcessing",
                data: "{'guid': '<%= this.UploadFile.GuidString %>'}",
	            contentType: "application/json; charset=utf-8",
	            dataType: "json",
	            success: function (msg) {
	                setTimeout('updateProgressProcessing();', 1000);
	            }
	        });
            
            // Set timeout to update progress bar
        }
        
        function updateProgressProcessing() {

	        $.ajax({
	            type: "POST",
	            url: "ResyncExternalAccount.aspx/GetProcessingProgress",
                data: "{'guid': '<%= this.UploadFile.GuidString %>'}",
	            contentType: "application/json; charset=utf-8",
	            dataType: "json",
	            success: function (msg) {
	                if (msg.d > 99) {
	                    $("#DivProgressProcessing .progressbar-value").animate(
                            {
                                width: "100%"
                            }, { queue: false });
	                    $("#tableResyncPreview").treegrid(
	                        {url: 'Json-ResyncPreview.aspx?Guid=<%= this.UploadFile.GuidString %>'});
	                    $('#DivStepPreview').fadeIn();
	                    $('#DivStepProcessing').slideUp();
	                } else {
	                    
	                    // We're not done yet. Keep the progress bar on-screen and keep re-checking.

	                    if (msg.d == 1) {
	                        $('#DivProgressProcessing').progressbar( { value: 1 });
	                    } else {
	                        $("#DivProgressProcessing .progressbar-value").animate(
                            {
                                width: msg.d + "%"
                            }, { queue: false });
	                    }
	                    
                        if (msg.d > 0 && !progressReceived) {
                            progressReceived = true;
                            $.ajax({
                                type: "POST",
                                url: "ResyncExternalAccount.aspx/GetProcessingStatistics",
                                data: "{'guid': '<%= this.UploadFile.GuidString %>'}",
                                contentType: "application/json; charset=utf-8",
                                dataType: "json",
                                success: function(msg2) {
                                    $('#SpanFirstTx').text(msg2.d.FirstTransaction);
                                    $('#SpanLastTx').text(msg2.d.LastTransaction);
                                    $('#SpanTxCount').text(msg2.d.TransactionCount);
                                }
                            });
            	        }

	                    setTimeout('updateProgressProcessing();', 1000);
	                }
	            }
	        });
	    }

        var progressReceived = false;

	    var currentYear = <%=DateTime.Today.Year %>;

	</script>
    <style>
	    .content h2 select {
		    font-size: 16px;
            font-weight: bold;
            color: #1C397E;
            letter-spacing: 1px;
	    }
    </style>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="PlaceHolderMain" Runat="Server">
    <div id="DivStepUpload">
    <h2>Step 1/4: Upload Master File</h2>
    <div id="DivPrepData">
        
        <div class="entryFields">
            <asp:DropDownList runat="server" ID="DropAccounts"/>&nbsp;
            <Swarmops5:FileUpload runat="server" ID="UploadFile" Filter="NoFilter" DisplayCount="8" HideTrigger="true" ClientUploadCompleteCallback="uploadCompletedCallback" /></div>
        
        <div class="entryLabels">
            Account to resync<br/>
            Upload master file
        </div>
    </div>
    
    <br clear="all"/>
    </div>
    
    <div id="DivStepProcessing" style="display:none">
    <h2>Step 2/4: Comparing records to master...</h2>
    <div id="DivProgressProcessing" style="width:100%"></div>
    <ul style="margin-left:20px"><li>The first transaction in the master file was on <span id="SpanFirstTx">[...]</span>.</li>
    <li>The last transaction in master file was on <span id="SpanLastTx">[...]</span>.</li>
    <li>There are <span id="SpanTxCount">[...]</span> transactions in the master file.</li>
    </ul>
    </div>
    
    <div id="DivStepPreview" style="display:none">
        <h2>Step 3/4: Comparison results - Resync?</h2>
    
        <table id="tableResyncPreview" title="" class="easyui-treegrid" style="width:680px;height:600px"  
            url=""
            rownumbers="false"
            animate="true"
            fitColumns="true"
            idField="id" treeField="rowName">
            <thead>  
                <tr>  
                    <th field="rowName" width="180">Date / Transaction</th>  
                    <th field="swarmopsData" width="140" align="left">Our Database</th>
                    <th field="masterData" width="140" align="left">Master File</span></th>
                    <th field="resyncAction" width="120" align="left">Resync Action</span></th>
                    <th field="notes" width="100" align="left">Notes</th>  
                </tr>  
            </thead>  
        </table> 
    </div>
    
    <div id="DivStepResynchronizing" style="display:none">
        <h2>Step 4/4: Resynchronizing with master...</h2>
    </div>
    
    <div id="DivStepComplete" style="display:none">
        <h2>Resynchronization complete</h2>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="PlaceHolderSide" Runat="Server">

</asp:Content>
