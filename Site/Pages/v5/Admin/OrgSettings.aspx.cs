﻿using System;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Web.Services;
using System.Web.UI.WebControls;
using Swarmops.Common.Enums;
using Swarmops.Frontend.Controls.v5.Base;
using Swarmops.Logic.Communications;
using Swarmops.Logic.Financial;
using Swarmops.Logic.Security;
using Swarmops.Logic.Structure;
using Swarmops.Logic.Support;
using Swarmops.Logic.Swarm;

namespace Swarmops.Frontend.Pages.Admin
{
    public partial class OrgSettings : PageV5Base
    {
        protected void Page_Load (object sender, EventArgs e)
        {
            PageIcon = "iconshock-box-cog";
            PageTitle = Resources.Pages.Admin.EditOrganization_PageTitle;
            InfoBoxLiteral = Resources.Pages.Admin.EditOrganization_Info;
            PageAccessRequired = new Access (CurrentOrganization, AccessAspect.Administration, AccessType.Write);

            if (!Page.IsPostBack)
            {
                Localize();
            }

            RegisterControl (EasyUIControl.Tabs);
            RegisterControl (IncludedControl.SwitchButton);
        }

        private void Localize()
        {
            string participants = Participant.Localized (CurrentOrganization.RegularLabel, TitleVariant.Plural);
            string participantship = Participant.Localized (CurrentOrganization.RegularLabel, TitleVariant.Ship);

            this.LabelParticipationEntry.Text =
                String.Format (Resources.Pages.Admin.EditOrganization_ParticipationBeginsWhen, participants);
            this.LabelParticipationOrg.Text =
                String.Format (Resources.Pages.Admin.EditOrganization_ParticipationBeginsOrg, participants);
            this.LabelParticipationDuration.Text =
                String.Format (Resources.Pages.Admin.EditOrganization_ParticipationDuration, participantship);
            this.LabelParticipationChurn.Text =
                String.Format (Resources.Pages.Admin.EditOrganization_ParticipationChurn, participantship);
            this.LabelRenewalCost.Text =
                String.Format (Resources.Pages.Admin.EditOrganization_RenewalsCost,
                    CurrentOrganization.Currency.DisplayCode);
            this.LabelParticipationCost.Text =
                String.Format (Resources.Pages.Admin.EditOrganization_ParticipationCost,
                    participantship, CurrentOrganization.Currency.DisplayCode);

            this.LabelRenewalsAffect.Text = Resources.Pages.Admin.EditOrganization_RenewalsAffect;
            this.LabelRenewalDateEffect.Text = Resources.Pages.Admin.EditOrganization_RenewalDateEffect;
            this.LabelRenewalReminder.Text = Resources.Pages.Admin.EditOrganization_RenewalReminders;
            this.LabelMemberNumber.Text =
                String.Format (Resources.Pages.Admin.EditOrganization_MemberNumberStyle, participantship);

            // Premium features

            this.LabelVanityDomain.Text = Resources.Pages.Admin.EditOrganization_VanityDomain;
            this.LabelOpenLedgersDomain.Text = Resources.Pages.Admin.EditOrganization_OpenLedgersDomain;
            this.TextVanityDomain.Placeholder = Resources.Global.Global_DefineToEnable;
            this.TextOpenLedgersDomain.Placeholder = Resources.Global.Global_DefineToEnable;

            this.DropMembersWhen.Items.Clear();
            this.DropMembersWhen.Items.Add (new ListItem ("Application submitted", "Application"));
            this.DropMembersWhen.Items.Add (new ListItem ("Application approved", "ApplicationApproval"));
            this.DropMembersWhen.Items.Add (new ListItem ("Application submitted + paid", "ApplicationPayment"));
            this.DropMembersWhen.Items.Add (new ListItem ("Application paid + approved", "ApplicationPaymentApproval"));
            this.DropMembersWhen.Items.Add (new ListItem ("Invited and accepted", "InvitationAcceptance"));
            this.DropMembersWhen.Items.Add (new ListItem ("Invited and paid", "InvitationPayment"));
            this.DropMembersWhen.Items.Add (new ListItem ("Manual add only", "Manual"));

            this.DropMembersWhere.Items.Clear();
            this.DropMembersWhere.Items.Add (new ListItem ("Root organization only", "Root"));
            this.DropMembersWhere.Items.Add (new ListItem ("Most local org only", "Local"));
            this.DropMembersWhere.Items.Add (new ListItem ("Root and most local org", "RootLocal"));
            this.DropMembersWhere.Items.Add (new ListItem ("All applicable organizations", "All"));

            this.DropMembershipDuration.Items.Clear();
            this.DropMembershipDuration.Items.Add (new ListItem ("One month", "OneMonth"));
            this.DropMembershipDuration.Items.Add (new ListItem ("One year", "OneYear"));
            this.DropMembershipDuration.Items.Add (new ListItem ("Two years", "TwoYears"));
            this.DropMembershipDuration.Items.Add (new ListItem ("Five years", "FiveYears"));
            this.DropMembershipDuration.Items.Add (new ListItem ("Forever", "Forever"));
            this.DropMembershipDuration.SelectedValue = "Year";

            this.DropMembersChurn.Items.Clear();
            this.DropMembersChurn.Items.Add (new ListItem ("Expiry date reached", "Expiry"));
            this.DropMembersChurn.Items.Add (new ListItem ("Not paid final reminder", "NotPaid"));
            this.DropMembersChurn.Items.Add (new ListItem ("Never", "Never"));

            this.DropRenewalDateEffect.Items.Clear();
            this.DropRenewalDateEffect.Items.Add (new ListItem ("Date of renewal", "RenewalDate"));
            this.DropRenewalDateEffect.Items.Add (new ListItem ("Previous expiry", "FromExpiry"));

            this.DropRenewalsAffect.Items.Clear();
            this.DropRenewalsAffect.Items.Add (new ListItem ("All related organizations", "All"));
            this.DropRenewalsAffect.Items.Add (new ListItem ("One organization at a time", "One"));

            this.DropRenewalReminder.Items.Clear();
            this.DropRenewalReminder.Items.Add (new ListItem ("30, 14, 7, 1 days before", "Standard"));
            this.DropRenewalReminder.Items.Add (new ListItem ("Never", "Never"));

            this.DropMemberNumber.Items.Clear();
            this.DropMemberNumber.Items.Add (new ListItem ("Global for installation", "Global"));
            this.DropMemberNumber.Items.Add (new ListItem ("Local for each organzation", "Local"));

            this.DropTaxAuthority.Items.Clear();
            this.DropTaxAuthority.Items.Add(new ListItem("[DE] Germany", "DE"));
            this.DropTaxAuthority.Items.Add(new ListItem("[SE] Sweden", "SE"));
        }

        [WebMethod]
        public static InitialOrgData GetInitialData()
        {
            InitialOrgData result = new InitialOrgData();
            AuthenticationData authData = GetAuthenticationDataAndCulture();
            Organization org = authData.CurrentOrganization;

            if (org == null || authData.CurrentUser == null)
            {
                return result; // just... don't
            }

            result.AccountBitcoinCold = (org.FinancialAccounts.AssetsBitcoinCold != null &&
                                         org.FinancialAccounts.AssetsBitcoinCold.Active);
            result.AccountBitcoinHot = (org.FinancialAccounts.AssetsBitcoinHot != null &&
                                        org.FinancialAccounts.AssetsBitcoinHot.Active);
            result.AccountPaypal = (org.FinancialAccounts.AssetsPaypal != null &&
                                    org.FinancialAccounts.AssetsPaypal.Active);
            result.AccountsForex = (org.FinancialAccounts.IncomeCurrencyFluctuations != null &&
                                    org.FinancialAccounts.IncomeCurrencyFluctuations.Active);
            result.AccountsVat = (org.FinancialAccounts.AssetsVatInbound != null &&
                                  org.FinancialAccounts.AssetsVatInbound.Active);
            result.ParticipantFinancials = org.ParticipantFinancialsEnabled;
            result.PaypalAccountAddress = org.PaypalAccountMailAddress;

            result.GovernmentRegistrationId = org.GovernmentRegistrationId;
            result.TaxAuthority = org.TaxAuthority;
            result.TaxPaymentOcr = org.TaxPaymentOcr;

            result.OpenLedgersDomain = org.OpenLedgersDomain;
            result.VanityDomain = org.VanityDomain;

            // TODO: Add all the other fields

            return result;
        }


        [WebMethod]
        public static AjaxInputCallResult SwitchToggled (string cookie, bool newValue)
        {
            AuthenticationData authData = GetAuthenticationDataAndCulture();

            if (authData.CurrentOrganization == null || authData.CurrentUser == null)
            {
                return null; // just don't... don't anything, actually
            }

            AjaxInputCallResult result = new AjaxInputCallResult();
            result.Success = true;
            result.NewValue = newValue? "true": "false";

            bool bitcoinNative = (authData.CurrentOrganization.Currency.Code == "BTC");

            FinancialAccounts workAccounts = new FinancialAccounts();

            switch (cookie)
            {
                case "BitcoinCold":

                    FinancialAccount coldAccount = authData.CurrentOrganization.FinancialAccounts.AssetsBitcoinCold;
                    if (coldAccount == null)
                    {
                        coldAccount = FinancialAccount.Create (authData.CurrentOrganization, "[LOC]Asset_BitcoinCold",
                            FinancialAccountType.Asset, null);
                        FinancialAccount.Create (authData.CurrentOrganization, "Cold Address 1",
                            FinancialAccountType.Asset, coldAccount);
                        FinancialAccount.Create (authData.CurrentOrganization, "Cold Address 2 (rename these)",
                            FinancialAccountType.Asset, coldAccount);
                        FinancialAccount.Create (authData.CurrentOrganization, "Cold Address... etc",
                            FinancialAccountType.Asset, coldAccount);

                        authData.CurrentOrganization.FinancialAccounts.AssetsBitcoinCold = coldAccount;

                        result.DisplayMessage =
                            "Bitcoin cold accounts were created. Edit names and addresses in Account Plan."; // LOC
                    }
                    else
                    {
                        workAccounts.Add (coldAccount);
                    }
                    break;
                case "BitcoinHot":
                    FinancialAccount hotAccount = authData.CurrentOrganization.FinancialAccounts.AssetsBitcoinHot;
                    if (hotAccount == null)
                    {
                        authData.CurrentOrganization.FinancialAccounts.AssetsBitcoinHot =
                            FinancialAccount.Create (authData.CurrentOrganization, "[LOC]Asset_BitcoinHot",
                                FinancialAccountType.Asset, null);

                        result.DisplayMessage =
                            "Bitcoin HD hotwallet was created along with an account for the hotwallet.";
                    }
                    else
                    {
                        workAccounts.Add (hotAccount);
                    }
                    break;
                case "Forex":
                    FinancialAccount forexGain =
                        authData.CurrentOrganization.FinancialAccounts.IncomeCurrencyFluctuations;
                    FinancialAccount forexLoss =
                        authData.CurrentOrganization.FinancialAccounts.CostsCurrencyFluctuations;

                    if (forexGain == null)
                    {
                        if (forexLoss != null)
                        {
                            throw new InvalidOperationException();
                        }

                        authData.CurrentOrganization.FinancialAccounts.IncomeCurrencyFluctuations =
                            FinancialAccount.Create (authData.CurrentOrganization, "[LOC]Income_ForexGains",
                                FinancialAccountType.Income, null);
                        authData.CurrentOrganization.FinancialAccounts.CostsCurrencyFluctuations =
                            FinancialAccount.Create (authData.CurrentOrganization, "[LOC]Cost_ForexLosses",
                                FinancialAccountType.Cost, null);

                        result.DisplayMessage =
                            "Forex gain/loss accounts were created and will be used to account for currency fluctuations.";
                    }
                    else
                    {
                        if (forexLoss == null)
                        {
                            throw new InvalidOperationException();
                        }

                        if (!bitcoinNative && newValue == false &&
                            ((authData.CurrentOrganization.FinancialAccounts.AssetsBitcoinCold != null &&
                              authData.CurrentOrganization.FinancialAccounts.AssetsBitcoinCold.Active) ||
                             (authData.CurrentOrganization.FinancialAccounts.AssetsBitcoinHot != null &&
                              authData.CurrentOrganization.FinancialAccounts.AssetsBitcoinHot.Active)))
                        {
                            // bitcoin is active, and we're not bitcoin native, so we're not turning off forex

                            result.Success = false;
                            result.NewValue = "true";
                            result.DisplayMessage =
                                "Cannot disable forex: bitcoin accounts are active in a non-bitcoin-native organization.";
                        }
                        else
                        {
                            workAccounts.Add (forexGain);
                            workAccounts.Add (forexLoss);
                        }
                    }
                    break;
                case "Vat":
                    FinancialAccount vatInbound = authData.CurrentOrganization.FinancialAccounts.AssetsVatInbound;
                    FinancialAccount vatOutbound = authData.CurrentOrganization.FinancialAccounts.DebtsVatOutbound;

                    if (vatInbound == null)
                    {
                        if (vatOutbound != null)
                        {
                            throw new InvalidOperationException();
                        }

                        authData.CurrentOrganization.FinancialAccounts.AssetsVatInbound =
                            FinancialAccount.Create (authData.CurrentOrganization, "[LOC]Asset_InboundVat",
                                FinancialAccountType.Asset, null);
                        authData.CurrentOrganization.FinancialAccounts.DebtsVatOutbound =
                            FinancialAccount.Create (authData.CurrentOrganization, "[LOC]Debt_OutboundVat",
                                FinancialAccountType.Debt, null);

                        result.DisplayMessage = "Inbound and outbound VAT accounts were created.";
                    }
                    else
                    {
                        if (vatOutbound == null)
                        {
                            throw new InvalidOperationException();
                        }

                        workAccounts.Add (vatInbound);
                        workAccounts.Add (vatOutbound);
                    }
                    break;
                case "Paypal":
                    FinancialAccount assetsPaypal = authData.CurrentOrganization.FinancialAccounts.AssetsPaypal;
                    if (assetsPaypal == null)
                    {
                        authData.CurrentOrganization.FinancialAccounts.AssetsPaypal =
                            FinancialAccount.Create (authData.CurrentOrganization, "[LOC]Asset_Paypal",
                                FinancialAccountType.Asset, null);

                        result.DisplayMessage = "An account was created for Paypal account tracking.";
                    }
                    else
                    {
                        workAccounts.Add (assetsPaypal);
                    }
                    break;
                case "ParticipantFinancials":
                    authData.CurrentOrganization.ParticipantFinancialsEnabled = newValue;
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (workAccounts.Count > 0 && String.IsNullOrEmpty (result.DisplayMessage))
            {
                if (newValue) // switch has been turned on
                {
                    // accounts can always be re-enabled. This is not a create, it is a re-enable.

                    foreach (FinancialAccount account in workAccounts)
                    {
                        account.Active = true;
                    }

                    if (workAccounts.Count > 1)
                    {
                        result.DisplayMessage = "The accounts were re-enabled.";
                    }
                    else
                    {
                        result.DisplayMessage = "The account was re-enabled.";
                    }
                }
                else // switch is being set to off position
                {
                    // if the accounts are currently enabled, we must first check there aren't
                    // any transactions in them before disabling
                    bool transactionsOnAccount = false;

                    foreach (FinancialAccount account in workAccounts)
                    {
                        if (account.GetLastRows (5).Count > 0)
                        {
                            transactionsOnAccount = true;
                        }
                    }

                    if (transactionsOnAccount)
                    {
                        if (workAccounts.Count > 1)
                        {
                            result.DisplayMessage = "Can't disable these accounts: there are transactions";
                        }
                        else
                        {
                            result.DisplayMessage = "Can't disable this account: there are transactions";
                        }

                        result.Success = false;
                        result.NewValue = "true";
                    }
                    else
                    {
                        // Disable accounts

                        foreach (FinancialAccount account in workAccounts)
                        {
                            account.Active = false;
                        }

                        if (workAccounts.Count > 1)
                        {
                            result.DisplayMessage = "The accounts were disabled.";
                        }
                        else
                        {
                            result.DisplayMessage = "The account was disabled.";
                        }
                    }
                }
            }

            return result;
        }


        public string OrganizationBitcoinNative
        {
            get
            {
                return CurrentOrganization.Currency.IsBitcoin.ToString().ToLowerInvariant(); // the tostring, tolower makes 'False' into 'false' the way JS wants it
            }
        }


        [WebMethod]
        static public AjaxInputCallResult StoreCallback(string newValue, string cookie)
        {
            AjaxInputCallResult result = new AjaxInputCallResult();
            AuthenticationData authenticationData = GetAuthenticationDataAndCulture();

            if (!authenticationData.Authority.HasAccess (new Access (authenticationData.CurrentOrganization, AccessAspect.Administration, AccessType.Write)))
            {
                result.Success = false; // this is the default on initialization, but let's be explicit about it
                result.FailReason = AjaxInputCallResult.ErrorAccessDenied;
                return result;
            }

            switch (cookie)
            {
                case "VanityDomain":
                    result.Success = true;
                    result.NewValue = newValue.Trim();
                    authenticationData.CurrentOrganization.VanityDomain = result.NewValue;
                    break;

                case "OpenLedgersDomain":
                    result.Success = true;
                    result.NewValue = newValue.Trim();
                    authenticationData.CurrentOrganization.OpenLedgersDomain = result.NewValue;
                    break;

                case "PaypalAccountAddress":
                    result.Success = true;
                    result.NewValue = newValue.Trim();
                    authenticationData.CurrentOrganization.PaypalAccountMailAddress = result.NewValue;
                    break;

                case "GovernmentRegistrationId":
                    result.Success = true;
                    result.NewValue = newValue.Trim();
                    authenticationData.CurrentOrganization.GovernmentRegistrationId = result.NewValue;
                    break;

                case "TaxPaymentOcr":
                    result.Success = true;
                    result.NewValue = newValue.Trim();
                    authenticationData.CurrentOrganization.TaxPaymentOcr = result.NewValue;
                    break;

                default:
                    throw new NotImplementedException("Unknown cookie in StoreCallback");
            }

            return result;
        }


        public class CallResult
        {
            public bool Success { get; set; }
            public string OpResult { get; set; }
            public string DisplayMessage { get; set; }
            public bool RequireForex { get; set; }
        }


        public class InitialOrgData
        {
            public bool AccountBitcoinCold;
            public bool AccountBitcoinHot;
            public bool AccountPaypal;
            public bool AccountsForex;
            public bool AccountsVat;
            public bool ParticipantFinancials;
            public string PaypalAccountAddress;

            public string GovernmentRegistrationId;
            public string TaxAuthority;
            public string TaxPaymentOcr;

            public string OpenLedgersDomain;
            public string VanityDomain;
        }
    }
}