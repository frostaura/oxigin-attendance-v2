import { configureStore } from '@reduxjs/toolkit';
import authReducer from './authSlice';
import timeEntryReducer from './timeEntrySlice';
import onboardingReducer from './onboardingSlice';

export const store = configureStore({
  reducer: {
    auth: authReducer,
    timeEntry: timeEntryReducer,
    onboarding: onboardingReducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        ignoredActions: ['persist/PERSIST', 'persist/REHYDRATE'],
      },
    }),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;