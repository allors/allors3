namespace Allors.Domain
{
		public interface Cacheable  : Object 
		{
						global::System.Guid CacheId {set;}

		}
		public interface Enumeration  : UniquelyIdentifiable 
		{
						global::System.String Name {set;}

						LocalisedText[] LocalisedNames {set;}

						global::System.Boolean? IsActive {set;}

		}
		public interface Version  : Object 
		{
						global::System.Guid? DerivationId {set;}

						global::System.DateTime? DerivationTimeStamp {set;}

		}
		public interface Versioned  : Object 
		{
		}
		public interface Printable  : Object 
		{
						PrintDocument PrintDocument {set;}

		}
		public interface Localised  : Object 
		{
						Locale Locale {set;}

		}
		public interface Auditable  : Object 
		{
						User CreatedBy {set;}

						User LastModifiedBy {set;}

						global::System.DateTime? CreationDate {set;}

						global::System.DateTime? LastModifiedDate {set;}

		}
		public interface ApproveTask  : Task 
		{
						global::System.String Comment {set;}

						Notification _RejectionNotification {set;}

						Notification _ApprovalNotification {set;}

		}
		public interface ObjectState  : UniquelyIdentifiable 
		{
						Permission[] ObjectDeniedPermissions {set;}

						global::System.String Name {set;}

		}
		public interface Task  : UniquelyIdentifiable, Deletable 
		{
						WorkItem _WorkItem {set;}

						global::System.String _Title {set;}

						global::System.DateTime? _DateCreated {set;}

						global::System.DateTime? _DateDue {set;}

						global::System.DateTime? DateClosed {set;}

						User[] _Participants {set;}

						User _Performer {set;}

		}
		public interface Transitional  : Object 
		{
						ObjectState[] _PreviousObjectStates {set;}

						ObjectState[] _LastObjectStates {set;}

						ObjectState[] _ObjectStates {set;}

						Permission[] _TransitionalDeniedPermissions {set;}

		}
		public interface TransitionalVersion  : Object 
		{
						ObjectState[] __PreviousObjectStates {set;}

						ObjectState[] __LastObjectStates {set;}

						ObjectState[] __ObjectStates {set;}

		}
		public interface User  : Deletable, UniquelyIdentifiable, SecurityTokenOwner 
		{
						NotificationList NotificationList {set;}

						global::System.String UserName {set;}

						global::System.String _NormalizedUserName {set;}

						global::System.String InUserPassword {set;}

						global::System.String UserPasswordHash {set;}

						global::System.String UserEmail {set;}

						global::System.String _NormalizedUserEmail {set;}

						global::System.Boolean UserEmailConfirmed {set;}

						global::System.String UserSecurityStamp {set;}

						global::System.String UserPhoneNumber {set;}

						global::System.Boolean UserPhoneNumberConfirmed {set;}

						global::System.Boolean UserTwoFactorEnabled {set;}

						global::System.DateTime? UserLockoutEnd {set;}

						global::System.Boolean UserLockoutEnabled {set;}

						global::System.Int32 UserAccessFailedCount {set;}

						Login[] Logins {set;}

		}
		public interface WorkItem  : Object 
		{
						global::System.String _WorkItemDescription {set;}

		}
		public interface AccessInterface  : DelegatedAccessControlledObject 
		{
		}
		public interface Address  : Object 
		{
						Place Place {set;}

		}
		public interface Addressable  : Object 
		{
						Address _Address {set;}

		}
		public interface DerivationCounted  : Object 
		{
						global::System.Int32 DerivationCount {set;}

		}
		public interface I1  : I12, S1 
		{
						I1 I1I1Many2One {set;}

						I12[] I1I12Many2Manies {set;}

						I2[] I1I2Many2Manies {set;}

						I2 I1I2Many2One {set;}

						global::System.String I1AllorsString {set;}

						I12 I1I12Many2One {set;}

						global::System.DateTime? I1AllorsDateTime {set;}

						I2[] I1I2One2Manies {set;}

						C2[] I1C2One2Manies {set;}

						C1 I1C1One2One {set;}

						global::System.Int32? I1AllorsInteger {set;}

						C2[] I1C2Many2Manies {set;}

						I1[] I1I1One2Manies {set;}

						I1[] I1I1Many2Manies {set;}

						global::System.Boolean? I1AllorsBoolean {set;}

						global::System.Decimal? I1AllorsDecimal {set;}

						I12 I1I12One2One {set;}

						I2 I1I2One2One {set;}

						C2 I1C2One2One {set;}

						C1[] I1C1One2Manies {set;}

						global::System.Byte[] I1AllorsBinary {set;}

						C1[] I1C1Many2Manies {set;}

						global::System.Double? I1AllorsDouble {set;}

						I1 I1I1One2One {set;}

						C1 I1C1Many2One {set;}

						I12[] I1I12One2Manies {set;}

						C2 I1C2Many2One {set;}

						global::System.Guid? I1AllorsUnique {set;}

		}
		public interface I12  : Object 
		{
						global::System.Byte[] I12AllorsBinary {set;}

						C2 I12C2One2One {set;}

						global::System.Double? I12AllorsDouble {set;}

						I1 I12I1Many2One {set;}

						global::System.String I12AllorsString {set;}

						I12[] I12I12Many2Manies {set;}

						global::System.Decimal? I12AllorsDecimal {set;}

						I2[] I12I2Many2Manies {set;}

						C2[] I12C2Many2Manies {set;}

						I1[] I12I1Many2Manies {set;}

						I12[] I12I12One2Manies {set;}

						global::System.String Name {set;}

						C1[] I12C1Many2Manies {set;}

						I2 I12I2Many2One {set;}

						global::System.Guid? I12AllorsUnique {set;}

						global::System.Int32? I12AllorsInteger {set;}

						I1[] I12I1One2Manies {set;}

						C1 I12C1One2One {set;}

						I12 I12I12One2One {set;}

						I2 I12I2One2One {set;}

						I12[] Dependencies {set;}

						I2[] I12I2One2Manies {set;}

						C2 I12C2Many2One {set;}

						I12 I12I12Many2One {set;}

						global::System.Boolean? I12AllorsBoolean {set;}

						I1 I12I1One2One {set;}

						C1[] I12C1One2Manies {set;}

						C1 I12C1Many2One {set;}

						global::System.DateTime? I12AllorsDateTime {set;}

		}
		public interface I2  : I12 
		{
						I2 I2I2Many2One {set;}

						C1 I2C1Many2One {set;}

						I12 I2I12Many2One {set;}

						global::System.Boolean? I2AllorsBoolean {set;}

						C1[] I2C1One2Manies {set;}

						C1 I2C1One2One {set;}

						global::System.Decimal? I2AllorsDecimal {set;}

						I2[] I2I2Many2Manies {set;}

						global::System.Byte[] I2AllorsBinary {set;}

						global::System.Guid? I2AllorsUnique {set;}

						I1 I2I1Many2One {set;}

						global::System.DateTime? I2AllorsDateTime {set;}

						I12[] I2I12One2Manies {set;}

						I12 I2I12One2One {set;}

						C2[] I2C2Many2Manies {set;}

						I1[] I2I1Many2Manies {set;}

						C2 I2C2Many2One {set;}

						global::System.String I2AllorsString {set;}

						C2[] I2C2One2Manies {set;}

						I1 I2I1One2One {set;}

						I1[] I2I1One2Manies {set;}

						I12[] I2I12Many2Manies {set;}

						I2 I2I2One2One {set;}

						global::System.Int32? I2AllorsInteger {set;}

						I2[] I2I2One2Manies {set;}

						C1[] I2C1Many2Manies {set;}

						C2 I2C2One2One {set;}

						global::System.Double? I2AllorsDouble {set;}

		}
		public interface S1  : Object 
		{
		}
		public interface Shared  : Object 
		{
		}
		public interface SyncDepthI1  : DerivationCounted 
		{
						SyncDepth2 __SyncDepth2 {set;}

						global::System.Int32 Value {set;}

		}
		public interface ValidationI12  : Object 
		{
						global::System.Guid? UniqueId {set;}

		}
		public interface Deletable  : Object 
		{
		}
		public interface Object 
		{
						Permission[] _DeniedPermissions {set;}

						SecurityToken[] SecurityTokens {set;}

		}
		public interface UniquelyIdentifiable  : Object 
		{
						global::System.Guid UniqueId {set;}

		}
		public interface DelegatedAccessControlledObject  : Object 
		{
		}
		public interface Permission  : Deletable 
		{
						global::System.Guid ClassPointer {set;}

		}
		public interface SecurityTokenOwner  : Object 
		{
						SecurityToken _OwnerSecurityToken {set;}

						AccessControl _OwnerAccessControl {set;}

		}
		public interface Counter  : UniquelyIdentifiable 
		{
						global::System.Int32 Value {set;}

		}
		public interface Singleton  : Object 
		{
						Locale DefaultLocale {set;}

						Locale[] AdditionalLocales {set;}

						Locale[] _Locales {set;}

						Media LogoImage {set;}

		}
		public interface Media  : UniquelyIdentifiable, Deletable 
		{
						global::System.Guid? _Revision {set;}

						MediaContent MediaContent {set;}

						global::System.String InType {set;}

						global::System.Byte[] InData {set;}

						global::System.String InDataUri {set;}

						global::System.String InFileName {set;}

						global::System.String Name {set;}

						global::System.String _Type {set;}

						global::System.String _FileName {set;}

		}
		public interface MediaContent  : Deletable 
		{
						global::System.String Type {set;}

						global::System.Byte[] Data {set;}

		}
		public interface PrintDocument  : Deletable 
		{
						Media Media {set;}

		}
		public interface Template  : UniquelyIdentifiable, Deletable 
		{
						TemplateType TemplateType {set;}

						Media Media {set;}

						global::System.String Arguments {set;}

		}
		public interface TemplateType  : Enumeration, Deletable 
		{
		}
		public interface PersistentPreparedExtent  : UniquelyIdentifiable, Deletable 
		{
						global::System.String Name {set;}

						global::System.String Description {set;}

						global::System.String Content {set;}

		}
		public interface PersistentPreparedSelect  : UniquelyIdentifiable, Deletable 
		{
						global::System.String Name {set;}

						global::System.String Description {set;}

						global::System.String Content {set;}

		}
		public interface Country  : Object 
		{
						Currency Currency {set;}

						global::System.String IsoCode {set;}

						global::System.String Name {set;}

						LocalisedText[] LocalisedNames {set;}

		}
		public interface Currency  : Enumeration 
		{
						global::System.String IsoCode {set;}

		}
		public interface Language  : Object 
		{
						global::System.String IsoCode {set;}

						global::System.String Name {set;}

						LocalisedText[] LocalisedNames {set;}

						global::System.String NativeName {set;}

		}
		public interface Locale  : Object 
		{
						global::System.String Name {set;}

						Language Language {set;}

						Country Country {set;}

		}
		public interface LocalisedMedia  : Localised, Deletable 
		{
						Media Media {set;}

		}
		public interface LocalisedText  : Localised, Deletable 
		{
						global::System.String Text {set;}

		}
		public interface AutomatedAgent  : User 
		{
						global::System.String Name {set;}

						global::System.String Description {set;}

		}
		public interface Person  : User, Addressable 
		{
						global::System.String FirstName {set;}

						global::System.String MiddleName {set;}

						global::System.String LastName {set;}

						global::System.Int32? Age {set;}

						global::System.DateTime? BirthDate {set;}

						global::System.String _FullName {set;}

						global::System.String _LocalFullName {set;}

						global::System.String _WorkingFullName {set;}

						global::System.String _DomainFullName {set;}

						global::System.String _DomainGreeting {set;}

						Gender Gender {set;}

						global::System.Boolean? IsMarried {set;}

						global::System.Boolean? IsStudent {set;}

						MailboxAddress MailboxAddress {set;}

						Address MainAddress {set;}

						Media Photo {set;}

						Media[] Pictures {set;}

						global::System.Int32? ShirtSize {set;}

						global::System.String Text {set;}

						global::System.String TinyMCEText {set;}

						global::System.Decimal? Weight {set;}

						Organisation CycleOne {set;}

						Organisation[] CycleMany {set;}

		}
		public interface EmailMessage  : Object 
		{
						global::System.DateTime _DateCreated {set;}

						global::System.DateTime? DateSending {set;}

						global::System.DateTime? DateSent {set;}

						User Sender {set;}

						User[] Recipients {set;}

						global::System.String RecipientEmailAddress {set;}

						global::System.String Subject {set;}

						global::System.String Body {set;}

		}
		public interface Notification  : Deletable 
		{
						UniquelyIdentifiable Target {set;}

						global::System.Boolean Confirmed {set;}

						global::System.String Title {set;}

						global::System.String Description {set;}

						global::System.DateTime _DateCreated {set;}

		}
		public interface NotificationList  : Deletable 
		{
						Notification[] Notifications {set;}

						Notification[] _UnconfirmedNotifications {set;}

						Notification[] _ConfirmedNotifications {set;}

		}
		public interface TaskAssignment  : Deletable 
		{
						User User {set;}

						Notification Notification {set;}

						Task Task {set;}

		}
		public interface AccessClass  : AccessInterface 
		{
						global::System.Boolean Block {set;}

						global::System.String Property {set;}

		}
		public interface BadUI  : Object 
		{
						Person[] PersonsMany {set;}

						Organisation CompanyOne {set;}

						Person PersonOne {set;}

						Organisation CompanyMany {set;}

						global::System.String AllorsString {set;}

		}
		public interface Build  : Object 
		{
						global::System.Guid? Guid {set;}

						global::System.String String {set;}

		}
		public interface C1  : I1, DerivationCounted 
		{
						global::System.Byte[] C1AllorsBinary {set;}

						global::System.Boolean? C1AllorsBoolean {set;}

						global::System.DateTime? C1AllorsDateTime {set;}

						global::System.Decimal? C1AllorsDecimal {set;}

						global::System.Double? C1AllorsDouble {set;}

						global::System.Int32? C1AllorsInteger {set;}

						global::System.String C1AllorsString {set;}

						global::System.String AllorsStringMax {set;}

						global::System.Guid? C1AllorsUnique {set;}

						C1[] C1C1Many2Manies {set;}

						C1 C1C1Many2One {set;}

						C1[] C1C1One2Manies {set;}

						C1 C1C1One2One {set;}

						C2[] C1C2Many2Manies {set;}

						C2 C1C2Many2One {set;}

						C2[] C1C2One2Manies {set;}

						C2 C1C2One2One {set;}

						I12[] C1I12Many2Manies {set;}

						I12 C1I12Many2One {set;}

						I12[] C1I12One2Manies {set;}

						I12 C1I12One2One {set;}

						I1[] C1I1Many2Manies {set;}

						I1 C1I1Many2One {set;}

						I1[] C1I1One2Manies {set;}

						I1 C1I1One2One {set;}

						I2[] C1I2Many2Manies {set;}

						I2 C1I2Many2One {set;}

						I2[] C1I2One2Manies {set;}

						I2 C1I2One2One {set;}

		}
		public interface C2  : DerivationCounted, I2 
		{
						global::System.Decimal? C2AllorsDecimal {set;}

						C1 C2C1One2One {set;}

						C2 C2C2Many2One {set;}

						global::System.Guid? C2AllorsUnique {set;}

						I12 C2I12Many2One {set;}

						I12 C2I12One2One {set;}

						I1[] C2I1Many2Manies {set;}

						global::System.Double? C2AllorsDouble {set;}

						I1[] C2I1One2Manies {set;}

						I2 C2I2One2One {set;}

						global::System.Int32? C2AllorsInteger {set;}

						I2[] C2I2Many2Manies {set;}

						I12[] C2I12Many2Manies {set;}

						C2[] C2C2One2Manies {set;}

						global::System.Boolean? C2AllorsBoolean {set;}

						I1 C2I1Many2One {set;}

						I1 C2I1One2One {set;}

						C1[] C2C1Many2Manies {set;}

						I12[] C2I12One2Manies {set;}

						I2[] C2I2One2Manies {set;}

						C2 C2C2One2One {set;}

						global::System.String C2AllorsString {set;}

						C1 C2C1Many2One {set;}

						C2[] C2C2Many2Manies {set;}

						global::System.DateTime? C2AllorsDateTime {set;}

						I2 C2I2Many2One {set;}

						C1[] C2C1One2Manies {set;}

						global::System.Byte[] C2AllorsBinary {set;}

						S1 S1One2One {set;}

		}
		public interface ClassWithoutRoles  : Object 
		{
		}
		public interface Data  : Object 
		{
						Person AutocompleteFilter {set;}

						Person AutocompleteOptions {set;}

						global::System.Boolean? Checkbox {set;}

						Person[] Chips {set;}

						global::System.String String {set;}

						global::System.Decimal? Decimal {set;}

						global::System.DateTime? Date {set;}

						global::System.DateTime? DateTime {set;}

						global::System.DateTime? DateTime2 {set;}

						Media File {set;}

						Media[] MultipleFiles {set;}

						global::System.String RadioGroup {set;}

						global::System.Int32? Slider {set;}

						global::System.Boolean? SlideToggle {set;}

						global::System.String PlainText {set;}

						global::System.String Markdown {set;}

						global::System.String Html {set;}

		}
		public interface Dependee  : DerivationCounted 
		{
						Subdependee Subdependee {set;}

						global::System.Int32? Subcounter {set;}

						global::System.Int32? Counter {set;}

						global::System.Boolean? DeleteDependent {set;}

		}
		public interface Dependent  : Deletable, DerivationCounted 
		{
						Dependee Dependee {set;}

						global::System.Int32? Counter {set;}

						global::System.Int32? Subcounter {set;}

		}
		public interface Extender  : Object 
		{
						global::System.String AllorsString {set;}

		}
		public interface First  : DerivationCounted 
		{
						Second Second {set;}

						global::System.Boolean? CreateCycle {set;}

						global::System.Boolean? IsDerived {set;}

		}
		public interface Four  : Shared 
		{
		}
		public interface From  : Object 
		{
						To[] Tos {set;}

		}
		public interface Gender  : Enumeration 
		{
		}
		public interface HomeAddress  : Address 
		{
						global::System.String Street {set;}

						global::System.String HouseNumber {set;}

		}
		public interface Left  : DerivationCounted 
		{
						Middle Middle {set;}

						global::System.Int32 Counter {set;}

						global::System.Boolean CreateMiddle {set;}

		}
		public interface MailboxAddress  : Address 
		{
						global::System.String PoBox {set;}

		}
		public interface MediaTyped  : Object 
		{
						global::System.String Markdown {set;}

		}
		public interface Middle  : DerivationCounted 
		{
						Right Right {set;}

						global::System.Int32 Counter {set;}

		}
		public interface One  : Shared 
		{
						Two Two {set;}

		}
		public interface Order  : Transitional, Versioned 
		{
						OrderState _PreviousOrderState {set;}

						OrderState _LastOrderState {set;}

						OrderState OrderState {set;}

						ShipmentState _PreviousShipmentState {set;}

						ShipmentState _LastShipmentState {set;}

						ShipmentState ShipmentState {set;}

						PaymentState _PreviousPaymentState {set;}

						PaymentState _LastPaymentState {set;}

						PaymentState PaymentState {set;}

						OrderLine[] OrderLines {set;}

						global::System.Decimal? Amount {set;}

						OrderState NonVersionedCurrentObjectState {set;}

						OrderLine[] NonVersionedOrderLines {set;}

						global::System.Decimal? NonVersionedAmount {set;}

						OrderVersion CurrentVersion {set;}

						OrderVersion[] AllVersions {set;}

		}
		public interface OrderLine  : Versioned 
		{
						global::System.Decimal? Amount {set;}

						OrderLineVersion CurrentVersion {set;}

						OrderLineVersion[] AllVersions {set;}

		}
		public interface OrderLineVersion  : Version 
		{
						global::System.Decimal? Amount {set;}

		}
		public interface OrderState  : ObjectState 
		{
		}
		public interface OrderVersion  : Version 
		{
						OrderState OrderState {set;}

						OrderLine[] OrderLines {set;}

						global::System.Decimal? _Amount {set;}

		}
		public interface Organisation  : Addressable, Deletable, UniquelyIdentifiable 
		{
						global::System.String Description {set;}

						Person[] Employees {set;}

						Person Manager {set;}

						Person Owner {set;}

						Person[] Shareholders {set;}

						Media[] Images {set;}

						global::System.Boolean? Incorporated {set;}

						global::System.DateTime? IncorporationDate {set;}

						global::System.String Information {set;}

						global::System.Boolean? IsSupplier {set;}

						Media Logo {set;}

						Address MainAddress {set;}

						global::System.String Name {set;}

						global::System.String Size {set;}

						Person CycleOne {set;}

						Person[] CycleMany {set;}

						Data OneData {set;}

						Data[] ManyDatas {set;}

						global::System.Boolean JustDidIt {set;}

		}
		public interface Page  : UniquelyIdentifiable 
		{
						global::System.String Name {set;}

						Media Content {set;}

		}
		public interface PaymentState  : ObjectState 
		{
		}
		public interface Place  : Object 
		{
						Country Country {set;}

						global::System.String City {set;}

						global::System.String PostalCode {set;}

		}
		public interface Post  : Object 
		{
						global::System.Int32 Counter {set;}

		}
		public interface Right  : DerivationCounted 
		{
						global::System.Int32 Counter {set;}

		}
		public interface Second  : DerivationCounted 
		{
						Third Third {set;}

						global::System.Boolean? IsDerived {set;}

		}
		public interface ShipmentState  : ObjectState 
		{
		}
		public interface SimpleJob  : Object 
		{
						global::System.Int32? Index {set;}

		}
		public interface StatefulCompany  : Object 
		{
						Person Employee {set;}

						global::System.String Name {set;}

						Person Manager {set;}

		}
		public interface Subdependee  : Object 
		{
						global::System.Int32? Subcounter {set;}

		}
		public interface SyncDepth2  : DerivationCounted 
		{
						global::System.Int32 Value {set;}

		}
		public interface SyncDepthC1  : SyncDepthI1 
		{
		}
		public interface SyncRoot  : DerivationCounted 
		{
						SyncDepthI1 __SyncDepth1 {set;}

						global::System.Int32 Value {set;}

		}
		public interface Third  : DerivationCounted 
		{
						global::System.Boolean? IsDerived {set;}

		}
		public interface Three  : Shared 
		{
						Four Four {set;}

						global::System.String AllorsString {set;}

		}
		public interface To  : Object 
		{
						global::System.String Name {set;}

		}
		public interface Two  : Shared 
		{
						Shared Shared {set;}

		}
		public interface UnitSample  : Object 
		{
						global::System.Byte[] AllorsBinary {set;}

						global::System.DateTime? AllorsDateTime {set;}

						global::System.Boolean? AllorsBoolean {set;}

						global::System.Double? AllorsDouble {set;}

						global::System.Int32? AllorsInteger {set;}

						global::System.String AllorsString {set;}

						global::System.Guid? AllorsUnique {set;}

						global::System.Decimal? AllorsDecimal {set;}

						global::System.Byte[] RequiredBinary {set;}

						global::System.DateTime RequiredDateTime {set;}

						global::System.Boolean RequiredBoolean {set;}

						global::System.Double RequiredDouble {set;}

						global::System.Int32 RequiredInteger {set;}

						global::System.String RequiredString {set;}

						global::System.Guid RequiredUnique {set;}

						global::System.Decimal RequiredDecimal {set;}

		}
		public interface ValidationC1  : ValidationI12 
		{
		}
		public interface ValidationC2  : ValidationI12 
		{
		}
		public interface WorkspaceNoneObject1  : Object 
		{
						global::System.String WorkspaceXString {set;}

						global::System.String WorkspaceYString {set;}

						global::System.String WorkspaceXYString {set;}

						global::System.String WorkspaceNonString {set;}

						WorkspaceXObject2[] WorkspaceXToWorkspaceXObject2 {set;}

						WorkspaceYObject2[] WorkspaceXToWorkspaceYObject2 {set;}

						WorkspaceXYObject2[] WorkspaceXToWorkspaceXYObject2 {set;}

						WorkspaceNonObject2[] WorkspaceXToWorkspaceNonObject2 {set;}

						WorkspaceXObject2[] WorkspaceYToWorkspaceXObject2 {set;}

						WorkspaceYObject2[] WorkspaceYToWorkspaceYObject2 {set;}

						WorkspaceXYObject2[] WorkspaceYToWorkspacXYObject2 {set;}

						WorkspaceNonObject2[] WorkspaceYToWorkspaceNonObject2 {set;}

						WorkspaceXObject2[] WorkspaceNoneToWorkspaceXObject2 {set;}

						WorkspaceYObject2[] WorkspaceNoneToWorkspaceYObject2 {set;}

						WorkspaceXYObject2[] WorkspaceNoneToWorkspaceXYObject2 {set;}

						WorkspaceNonObject2[] WorkspaceNoneToWorkspaceNonObject2 {set;}

		}
		public interface WorkspaceNonObject2  : Object 
		{
		}
		public interface WorkspaceXObject1  : Object 
		{
						global::System.String WorkspaceXString {set;}

						global::System.String WorkspaceYString {set;}

						global::System.String WorkspaceXYString {set;}

						global::System.String WorkspaceNonString {set;}

						WorkspaceXObject2[] WorkspaceXToWorkspaceXObject2 {set;}

						WorkspaceYObject2[] WorkspaceXToWorkspaceYObject2 {set;}

						WorkspaceXYObject2[] WorkspaceXToWorkspaceXYObject2 {set;}

						WorkspaceNonObject2[] WorkspaceXToWorkspaceNonObject2 {set;}

						WorkspaceXObject2[] WorkspaceYToWorkspaceXObject2 {set;}

						WorkspaceYObject2[] WorkspaceYToWorkspaceYObject2 {set;}

						WorkspaceXYObject2[] WorkspaceYToWorkspacXYObject2 {set;}

						WorkspaceNonObject2[] WorkspaceYToWorkspaceNonObject2 {set;}

						WorkspaceXObject2[] WorkspaceNoneToWorkspaceXObject2 {set;}

						WorkspaceYObject2[] WorkspaceNoneToWorkspaceYObject2 {set;}

						WorkspaceXYObject2[] WorkspaceNoneToWorkspaceXYObject2 {set;}

						WorkspaceNonObject2[] WorkspaceNoneToWorkspaceNonObject2 {set;}

		}
		public interface WorkspaceXObject2  : Object 
		{
		}
		public interface WorkspaceXYObject1  : Object 
		{
						global::System.String WorkspaceXString {set;}

						global::System.String WorkspaceYString {set;}

						global::System.String WorkspaceXYString {set;}

						global::System.String WorkspaceNonString {set;}

						WorkspaceXObject2[] WorkspaceXToWorkspaceXObject2 {set;}

						WorkspaceYObject2[] WorkspaceXToWorkspaceYObject2 {set;}

						WorkspaceXYObject2[] WorkspaceXToWorkspaceXYObject2 {set;}

						WorkspaceNonObject2[] WorkspaceXToWorkspaceNonObject2 {set;}

						WorkspaceXObject2[] WorkspaceYToWorkspaceXObject2 {set;}

						WorkspaceYObject2[] WorkspaceYToWorkspaceYObject2 {set;}

						WorkspaceXYObject2[] WorkspaceYToWorkspacXYObject2 {set;}

						WorkspaceNonObject2[] WorkspaceYToWorkspaceNonObject2 {set;}

						WorkspaceXObject2[] WorkspaceNoneToWorkspaceXObject2 {set;}

						WorkspaceYObject2[] WorkspaceNoneToWorkspaceYObject2 {set;}

						WorkspaceXYObject2[] WorkspaceNoneToWorkspaceXYObject2 {set;}

						WorkspaceNonObject2[] WorkspaceNoneToWorkspaceNonObject2 {set;}

		}
		public interface WorkspaceXYObject2  : Object 
		{
		}
		public interface WorkspaceYObject1  : Object 
		{
						global::System.String WorkspaceXString {set;}

						global::System.String WorkspaceYString {set;}

						global::System.String WorkspaceXYString {set;}

						global::System.String WorkspaceNonString {set;}

						WorkspaceXObject2[] WorkspaceXToWorkspaceXObject2 {set;}

						WorkspaceYObject2[] WorkspaceXToWorkspaceYObject2 {set;}

						WorkspaceXYObject2[] WorkspaceXToWorkspaceXYObject2 {set;}

						WorkspaceNonObject2[] WorkspaceXToWorkspaceNonObject2 {set;}

						WorkspaceXObject2[] WorkspaceYToWorkspaceXObject2 {set;}

						WorkspaceYObject2[] WorkspaceYToWorkspaceYObject2 {set;}

						WorkspaceXYObject2[] WorkspaceYToWorkspacXYObject2 {set;}

						WorkspaceNonObject2[] WorkspaceYToWorkspaceNonObject2 {set;}

						WorkspaceXObject2[] WorkspaceNoneToWorkspaceXObject2 {set;}

						WorkspaceYObject2[] WorkspaceNoneToWorkspaceYObject2 {set;}

						WorkspaceXYObject2[] WorkspaceNoneToWorkspaceXYObject2 {set;}

						WorkspaceNonObject2[] WorkspaceNoneToWorkspaceNonObject2 {set;}

		}
		public interface WorkspaceYObject2  : Object 
		{
		}
		public interface SessionOrganisation 
		{
						Person[] SessionDatabaseEmployees {set;}

						Person SessionDatabaseManager {set;}

						Person SessionDatabaseOwner {set;}

						Person[] SessionDatabaseShareholders {set;}

						WorkspacePerson[] SessionWorkspaceEmployees {set;}

						WorkspacePerson SessionWorkspaceManager {set;}

						WorkspacePerson SessionWorkspaceOwner {set;}

						WorkspacePerson[] SessionWorkspaceShareholders {set;}

						SessionPerson[] SessionSessionEmployees {set;}

						SessionPerson SessionSessionManager {set;}

						SessionPerson SessionSessionOwner {set;}

						SessionPerson[] SessionSessionShareholders {set;}

		}
		public interface SessionPerson 
		{
						global::System.String FirstName {set;}

						global::System.String LastName {set;}

						global::System.String _FullName {set;}

		}
		public interface WorkspaceOrganisation 
		{
						Person[] WorkspaceDatabaseEmployees {set;}

						Person WorkspaceDatabaseManager {set;}

						Person WorkspaceDatabaseOwner {set;}

						Person[] WorkspaceDatabaseShareholders {set;}

						WorkspacePerson[] WorkspaceWorkspaceEmployees {set;}

						WorkspacePerson WorkspaceWorkspaceManager {set;}

						WorkspacePerson WorkspaceWorkspaceOwner {set;}

						WorkspacePerson[] WorkspaceWorkspaceShareholders {set;}

		}
		public interface WorkspacePerson 
		{
						global::System.String FirstName {set;}

						global::System.String LastName {set;}

						global::System.String _FullName {set;}

		}
		public interface AccessControl  : Deletable, UniquelyIdentifiable 
		{
						UserGroup[] SubjectGroups {set;}

						User[] Subjects {set;}

						Role Role {set;}

						Permission[] _EffectivePermissions {set;}

						User[] _EffectiveUsers {set;}

		}
		public interface Login  : Deletable 
		{
						global::System.String Key {set;}

						global::System.String Provider {set;}

						global::System.String DisplayName {set;}

		}
		public interface CreatePermission  : Permission 
		{
		}
		public interface ExecutePermission  : Permission 
		{
						global::System.Guid MethodTypePointer {set;}

		}
		public interface ReadPermission  : Permission 
		{
						global::System.Guid RelationTypePointer {set;}

		}
		public interface WritePermission  : Permission 
		{
						global::System.Guid RelationTypePointer {set;}

		}
		public interface Role  : UniquelyIdentifiable 
		{
						Permission[] Permissions {set;}

						global::System.String Name {set;}

		}
		public interface SecurityToken  : Deletable, UniquelyIdentifiable 
		{
						AccessControl[] AccessControls {set;}

		}
		public interface UserGroup  : UniquelyIdentifiable 
		{
						User[] Members {set;}

						global::System.String Name {set;}

		}
}