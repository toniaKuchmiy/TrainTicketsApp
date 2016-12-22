﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using WpfApplication1.Model;

namespace WpfApplication1.ViewModel
{
    public class  AnotherViewModel : ObservableObject
    {
        private readonly string _username;
        private RelayCommand _searchCommand;

	    private string _startCityText;
		private string _endCityText;
		private DateTime _date;
	    private string _selectedItem;

	    private  RelayCommand _returnTicketCommand;

	    private  RelayCommand _createTicketCommand ;

	    private AggregationItem _selectedTicket;

	    public event EventHandler<string> ShowReturningTicketWindow;

	    public event EventHandler<AggregationItem> ShowAddingTicketWindow ; 

        public AnotherViewModel(string username)
        {
	        _username = username;
	      
			Date = new DateTime(2016, 8, 24);
			Tickets = new ObservableCollection<TICKET>();
			StartStations = new ObservableCollection<STATION>();
			EndStations = new ObservableCollection<STATION>();
			Places = new ObservableCollection<PLACE>();
			Carriages = new ObservableCollection<CARRIAGE>();
			Trains = new ObservableCollection<TRAIN>();
			ResultCollection = new ObservableCollection<AggregationItem>();
        }

	    public ObservableCollection<TRAIN> Trains { get; private set; }

	    public ObservableCollection<CARRIAGE> Carriages { get; private set; }

	    public ObservableCollection<PLACE> Places { get; private set; }

	    public ObservableCollection<STATION> StartStations { get; private set; }
		public ObservableCollection<AggregationItem> ResultCollection { get; private set; }

		public ObservableCollection<STATION> EndStations { get; private set; }
	   
		public string StartCityText
		{
			get { return _startCityText; }
			set
			{
				Set(ref _startCityText, value);
				SearchCommand.RaiseCanExecuteChanged();
			}
		}

		public string EndCityText
		{
			get { return _endCityText; }
			set
			{
				Set(ref _endCityText, value);
				SearchCommand.RaiseCanExecuteChanged();
			}
		}

		public DateTime Date
		{
			get { return _date; }
			set
			{
				Set(ref _date, value.Date);
			}
		}

	    public string SelectedItem
	    {
		    get
		    {
			    return _selectedItem;
		    }
		    set
		    {
			    Set(ref _selectedItem, value);
		    }
	    }

	    public AggregationItem SelectedTicket
	    {
		    get
		    {
			    return _selectedTicket;
		    }
		    set
		    {
			    Set(ref _selectedTicket, value);
		    }
		    
	    }

	    public ObservableCollection<TICKET> Tickets { get; private set; }

        public string Username
        {
            get { return _username; }
        }

	    public RelayCommand SearchCommand
        {
			get { return _searchCommand ?? (_searchCommand = new RelayCommand(Search, CanSearch)); }
        }

	    public RelayCommand ReturnTicketCommand
	    {
		    get
		    {
			    return _returnTicketCommand ?? (_returnTicketCommand=new RelayCommand(ReturnTicketGoWindow));
		    }
	    }

	    public RelayCommand CreateTicketCommand
	    {
		    get
		    {
			    return _createTicketCommand ?? (_createTicketCommand = new RelayCommand(OpenAddingTicketWindow));
		    }
	    }

	    private void ReturnTicketGoWindow()
	    {
		    OnShowReturningTicketWindow("eee");
	    }

	    private void OpenAddingTicketWindow()
	    {
			OnShowAddingTicketWindow(SelectedTicket);
	    }

	    private void Search()
        {
			Tickets.Clear();
			StartStations.Clear();
			EndStations.Clear();
			Places.Clear();
			Trains.Clear();
			Carriages.Clear();
			ResultCollection.Clear();

	        if (SelectedItem == null)
			        {
				        System.Windows.MessageBox.Show("Будь ласка, виберіть тип вагону.");
			        }

	        using (var context = new TrainContext())
	        {
		        var endStation = context.STATIONs.SingleOrDefault(c => c.NAME == EndCityText);

		        var startStation = context.STATIONs.SingleOrDefault(c => c.NAME == StartCityText);

		        var tickets = context.TICKETs.Where(c => c.TRAVEL_DATE == Date && c.STATE ).ToList();
				
		        foreach (TICKET ticket in tickets)
		        {
			    
					var train = context.TRAINS.SingleOrDefault(c => c.ID_START_STATION == startStation.ID && c.ID_END_STATION == endStation.ID);
			        
					var carriages = context.CARRIAGEs.Where(c => c.ID_TRAIN == train.ID && c.TYPE == SelectedItem && ticket.ID_TRAIN == train.ID).ToList();
					foreach (CARRIAGE carriage in carriages)
					{
						var places = context.PLACEs.Where(c => c.ID_CARRIAGE == carriage.ID && c.ID==ticket.ID_PLACE ).ToList();

						foreach (PLACE place in places)
						{
							ResultCollection.Add(
					        new AggregationItem
					        {
						        Carriage = carriage,
								Place = place,
								Ticket = ticket,
								Train = train,
								StartStation = startStation,
								EndStation = endStation
								
					        });
						}
					}
		        }
				
	        }
        }


		private bool CanSearch()
		{
			return !string.IsNullOrWhiteSpace(StartCityText) && !string.IsNullOrWhiteSpace(EndCityText) ;//&& !string.IsNullOrWhiteSpace(SelectedItem);
		}



        private void OnShowReturningTicketWindow(string e)
        {
            var handler = ShowReturningTicketWindow;
	        if (handler != null)
	        {
		        handler(this, e);
	        }
        }

	    protected virtual void OnShowAddingTicketWindow(AggregationItem e)
	    {
		    EventHandler<AggregationItem> handler = ShowAddingTicketWindow;
		    if (handler != null)
		    {
			    handler(this, e);
		    }
	    }
    }
}
