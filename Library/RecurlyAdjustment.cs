using System;
using System.Collections.Generic;
using System.Net;
using System.Xml;
using System.Text;

namespace Recurly
{
    public class RecurlyAdjustment
    {
        private const string UrlPostfix = "/adjustments";

        public string Id { get; protected set; }
        public int AmountInCents { get; protected set; }
        public int Quantity { get; protected set; }
        public DateTime StartDate { get; protected set; }
        public DateTime? EndDate { get; protected set; }
        public string Description { get; protected set; }
        public string Currency { get; protected set; }
        public string AccountingCode { get; protected set; }

        #region Constructors

        internal RecurlyAdjustment()
        {
            this.Quantity = 1;
            this.Currency = "USD";
        }

        internal RecurlyAdjustment(XmlTextReader xmlReader)
        {
            ReadXml(xmlReader);
        }

        #endregion

        public static RecurlyAdjustment ChargeAccount(string accountCode, int amountInCents, int quantity, string description)
        {
            RecurlyAdjustment adjustment = new RecurlyAdjustment();
            adjustment.AmountInCents = amountInCents;
            adjustment.Quantity = quantity;
            adjustment.StartDate = DateTime.UtcNow;
            adjustment.Description = description;

            /* HttpStatusCode statusCode = */
            RecurlyClient.PerformRequest(RecurlyClient.HttpRequestMethod.Post,
                ChargesUrl(accountCode),
                new RecurlyClient.WriteXmlDelegate(adjustment.WriteXml),
                null);

            return adjustment;
        }

        internal static string ChargesUrl(string accountCode)
        {
            return RecurlyAccount.UrlPrefixV2 + System.Web.HttpUtility.UrlEncode(accountCode) + UrlPostfix;
        }

        #region Read and Write XML documents

        internal void ReadXml(XmlTextReader reader)
        {
            while (reader.Read())
            {
                // End of account element, get out of here
                if ((reader.Name == "adjustment") &&
                    reader.NodeType == XmlNodeType.EndElement)
                    break;

                if (reader.NodeType == XmlNodeType.Element)
                {
                    DateTime date;
                    switch (reader.Name)
                    {
                        case "id":
                            this.Id = reader.ReadElementContentAsString();
                            break;

                        case "start_date":
                            if (DateTime.TryParse(reader.ReadElementContentAsString(), out date))
                                this.StartDate = date;
                            break;

                        case "end_date":
                            if (DateTime.TryParse(reader.ReadElementContentAsString(), out date))
                                this.EndDate = date;
                            break;

                        case "amount_in_cents":
                            int amount;
                            if (Int32.TryParse(reader.ReadElementContentAsString(), out amount))
                                this.AmountInCents = amount;
                            break;

                        case "quantity":
                            int quantity;
                            if (Int32.TryParse(reader.ReadElementContentAsString(), out quantity))
                                this.Quantity = quantity;
                            break;

                        case "description":
                            this.Description = reader.ReadElementContentAsString();
                            break;

                        case "currency":
                            this.Currency = reader.ReadElementContentAsString();
                            break;

                        case "accounting_code":
                            this.AccountingCode = reader.ReadElementContentAsString();
                            break;
                    }
                }
            }
        }

        protected string XmlRootNodeName { get { return "adjustment"; } }

        internal void WriteXml(XmlTextWriter xmlWriter)
        {
            xmlWriter.WriteStartElement(XmlRootNodeName); // Start: adjustment

            xmlWriter.WriteElementString("unit_amount_in_cents", this.AmountInCents.ToString());
            xmlWriter.WriteElementString("description", this.Description);
            xmlWriter.WriteElementString("quantity", this.Quantity.ToString());
            xmlWriter.WriteElementString("currency", this.Currency);
            xmlWriter.WriteElementString("accounting_code", this.AccountingCode);

            xmlWriter.WriteEndElement(); // End: adjustment
        }

        #endregion

        #region Object Overrides

        public override string ToString()
        {
            return "Recurly Adjustment: " + this.Id;
        }

        public override bool Equals(object obj)
        {
            if (obj is RecurlyAdjustment)
                return Equals((RecurlyAdjustment)obj);
            else
                return false;
        }

        public bool Equals(RecurlyAdjustment adjustment)
        {
            return this.Id == adjustment.Id;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        #endregion
    }
}