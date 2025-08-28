import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { TimeEntry, TimeReport } from '../types/timeEntry';

interface TimeEntryState {
  currentTimeEntry: TimeEntry | null;
  timeEntries: TimeEntry[];
  timeReport: TimeReport | null;
  allEmployeesReport: TimeReport[];
  loading: boolean;
  error: string | null;
}

const initialState: TimeEntryState = {
  currentTimeEntry: null,
  timeEntries: [],
  timeReport: null,
  allEmployeesReport: [],
  loading: false,
  error: null,
};

const timeEntrySlice = createSlice({
  name: 'timeEntry',
  initialState,
  reducers: {
    setLoading: (state, action: PayloadAction<boolean>) => {
      state.loading = action.payload;
    },
    setError: (state, action: PayloadAction<string | null>) => {
      state.error = action.payload;
    },
    setCurrentTimeEntry: (state, action: PayloadAction<TimeEntry | null>) => {
      state.currentTimeEntry = action.payload;
    },
    setTimeEntries: (state, action: PayloadAction<TimeEntry[]>) => {
      state.timeEntries = action.payload;
    },
    addTimeEntry: (state, action: PayloadAction<TimeEntry>) => {
      state.timeEntries.unshift(action.payload);
    },
    updateTimeEntry: (state, action: PayloadAction<TimeEntry>) => {
      const index = state.timeEntries.findIndex(entry => entry.id === action.payload.id);
      if (index !== -1) {
        state.timeEntries[index] = action.payload;
      }
      if (state.currentTimeEntry?.id === action.payload.id) {
        state.currentTimeEntry = action.payload;
      }
    },
    setTimeReport: (state, action: PayloadAction<TimeReport>) => {
      state.timeReport = action.payload;
    },
    setAllEmployeesReport: (state, action: PayloadAction<TimeReport[]>) => {
      state.allEmployeesReport = action.payload;
    },
    clearError: (state) => {
      state.error = null;
    },
    reset: (state) => {
      state.currentTimeEntry = null;
      state.timeEntries = [];
      state.timeReport = null;
      state.allEmployeesReport = [];
      state.loading = false;
      state.error = null;
    },
  },
});

export const {
  setLoading,
  setError,
  setCurrentTimeEntry,
  setTimeEntries,
  addTimeEntry,
  updateTimeEntry,
  setTimeReport,
  setAllEmployeesReport,
  clearError,
  reset,
} = timeEntrySlice.actions;

export default timeEntrySlice.reducer;