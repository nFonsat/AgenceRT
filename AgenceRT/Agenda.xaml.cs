using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using WinRTXamlToolkit.Controls;

using AgenceRT.AgenceWebService;
using System.Collections.ObjectModel;
using AgenceEntites;

// Pour en savoir plus sur le modèle d'élément Page de base, consultez la page http://go.microsoft.com/fwlink/?LinkId=234237

namespace AgenceRT
{
    /// <summary>
    /// Page de base qui inclut des caractéristiques communes à la plupart des applications.
    /// </summary>
    public sealed partial class Agenda : AgenceRT.Common.LayoutAwarePage
    {
        private ObservableCollection<AgendaEntite> _agendasAffiches = new ObservableCollection<AgendaEntite>();
        public ObservableCollection<AgendaEntite> AgendasAffiches
        {
            get { return _agendasAffiches; }
            set { _agendasAffiches = value; }
        }

        private ObservableCollection<AgendaEntite> _rdvCurrentDay = new ObservableCollection<AgendaEntite>();
        public ObservableCollection<AgendaEntite> RDVCurrentDay
        {
            get { return _rdvCurrentDay; }
            set { _rdvCurrentDay = value; }
        }

        public Agenda()
        {
            this.InitializeComponent();

            this.DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ChargerAgendas();
        }

        /// <summary>
        /// Remplit la page à l'aide du contenu passé lors de la navigation. Tout état enregistré est également
        /// fourni lorsqu'une page est recréée à partir d'une session antérieure.
        /// </summary>
        /// <param name="navigationParameter">Valeur de paramètre passée à
        /// <see cref="Frame.Navigate(Type, Object)"/> lors de la requête initiale de cette page.
        /// </param>
        /// <param name="pageState">Dictionnaire d'état conservé par cette page durant une session
        /// antérieure. Null lors de la première visite de la page.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Conserve l'état associé à cette page en cas de suspension de l'application ou de la
        /// suppression de la page du cache de navigation. Les valeurs doivent être conformes aux
        /// exigences en matière de sérialisation de <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">Dictionnaire vide à remplir à l'aide de l'état sérialisable.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private async void ChargerAgendas()
        {
            _agendasAffiches.Clear();

            ObservableCollection<AgendaDTO> agendas;
            AgenceWebServicesClient ws = new AgenceWebServicesClient();

            try
            {
                agendas = await ws.ChargerListeRendezVousAsync();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Erreur Charger Agenda : " + e.Message);
                return;
            }
            

            foreach (AgendaDTO agenda in agendas)
            {
                AgentEntite agent = new AgentEntite();
                try
                {
                    agent.Login = agenda.Agent.Login;
                    agent.MotDePasse = agenda.Agent.MotDePasse;
                }
                catch (Exception e)
                {
                    agent = new AgentEntite();
                    System.Diagnostics.Debug.WriteLine("Error AgentEntite : " + e.Message);
                }

                ProspectEntite prospect = new ProspectEntite();
                try
                {
                    prospect.DateContact = agenda.Prospect.DateContact;
                }
                catch (Exception e)
                {
                    prospect = new ProspectEntite();
                    System.Diagnostics.Debug.WriteLine("Error ProspectEntite : " + e.Message);
                }

                ProprietaireEntite proprietaire = new ProprietaireEntite();
                try
                {
                    proprietaire.Adresse = agenda.Annonce.Bien.Proprietaire.Adresse;
                }
                catch (Exception e)
                {
                    proprietaire = new ProprietaireEntite();
                    System.Diagnostics.Debug.WriteLine("Error ProprietaireEntite : " + e.Message);
                }

                BienEntite bien = new BienEntite();
                try
                {
                    bien.Adresse = agenda.Annonce.Bien.Adresse;
                    bien.Description = agenda.Annonce.Bien.Description;
                    bien.IdBien = agenda.Annonce.Bien.IdBien;
                    bien.IdTypeBien = agenda.Annonce.Bien.IdTypeBien;
                    bien.Latitude = agenda.Annonce.Bien.Latitude;
                    bien.LibelleType = agenda.Annonce.Bien.LibelleType;
                    bien.Longitude = agenda.Annonce.Bien.Longitude;
                    bien.Proprietaire = proprietaire;
                    bien.Titre = agenda.Annonce.Bien.Titre;
                }
                catch (Exception e)
                {
                    bien = new BienEntite();
                    System.Diagnostics.Debug.WriteLine("Error BienEntite : " + e.Message);
                }

                AnnonceEntite annonce = new AnnonceEntite();
                try
                {
                    annonce.IdAnnonce = agenda.Annonce.IdAnnonce;
                    annonce.Titre = agenda.Annonce.Titre;
                    annonce.Bien = bien;
                    annonce.Texte = agenda.Annonce.Texte;
                    annonce.Prix = agenda.Annonce.Prix;
                }
                catch (Exception e)
                {
                    annonce = new AnnonceEntite();
                    System.Diagnostics.Debug.WriteLine("Error AnnonceEntite : " + e.Message);
                }

                AgendaEntite agendaEntite = new AgendaEntite 
                { 
                    IdAgenda = agenda.IdAgenda, 
                    Date = agenda.Date,
                    Description = agenda.Description, 
                    Titre = agenda.Titre,
                    Agent = agent,
                    Prospect = prospect, 
                    Annonce = new AnnonceEntite()
                };

                _agendasAffiches.Add(agendaEntite);
            }
        }

        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            Calendar cal = (Calendar)sender;
            
            System.Diagnostics.Debug.WriteLine("Date Selected");
            System.Diagnostics.Debug.WriteLine(_agendasAffiches.Count);

            DateTime dateSelected = (DateTime)e.AddedItems[0];
            DateTime dateMax = new DateTime(dateSelected.Year, dateSelected.Month, dateSelected.Day + 1);

            _rdvCurrentDay.Clear();
            foreach(AgendaEntite rdv in _agendasAffiches)
            {
                if ((rdv.Date > dateSelected) && (rdv.Date < dateMax))
                {
                    _rdvCurrentDay.Add(rdv);
                }
            }
            
        }

        private void Calendar_DisplayDateChanged(object sender, CalendarDateChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Date Changed");
            DateTime date = e.AddedDate.Value;
            System.Diagnostics.Debug.WriteLine(date.Month + "/" + date.Year);
            
            
        }

        private void calendar_DisplayModeChanged(object sender, CalendarModeChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Mode Changed");
            System.Diagnostics.Debug.WriteLine(e.NewMode);
        }
    }
}
