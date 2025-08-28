import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface OnboardingStep {
  id: string;
  title: string;
  content: string;
  target?: string;
  page: string;
  action?: string;
}

interface OnboardingState {
  isActive: boolean;
  isCompleted: boolean;
  currentStep: number;
  currentPage: string;
  steps: OnboardingStep[];
}

const onboardingSteps: OnboardingStep[] = [
  // Dashboard steps
  {
    id: 'welcome',
    title: 'Welcome to Oxigin Attendance!',
    content: 'Let\'s take a quick tour to help you get started with tracking your time and managing attendance.',
    page: 'dashboard',
  },
  {
    id: 'navigation',
    title: 'Navigation Menu',
    content: 'Use this sidebar to navigate between different sections of the application. You can access Dashboard, Time Entries, Reports, and more.',
    target: 'nav-sidebar',
    page: 'dashboard',
  },
  {
    id: 'time-tracking',
    title: 'Time Tracking',
    content: 'This is your main time tracking card. Click "Clock In" to start tracking your work time, and "Clock Out" when you\'re done.',
    target: 'time-tracking-card',
    page: 'dashboard',
  },
  {
    id: 'quick-actions',
    title: 'Quick Actions',
    content: 'Access frequently used features quickly with these action buttons.',
    target: 'quick-actions-card',
    page: 'dashboard',
  },
  {
    id: 'profile-menu',
    title: 'Profile Menu',
    content: 'Click on your avatar to access profile settings, preferences, and logout options.',
    target: 'profile-avatar',
    page: 'dashboard',
  },
  {
    id: 'completion',
    title: 'You\'re All Set!',
    content: 'That\'s it! You\'re ready to start using Oxigin Attendance. Remember, you can always access help and documentation from the profile menu.',
    page: 'dashboard',
  },
];

const initialState: OnboardingState = {
  isActive: false,
  isCompleted: false,
  currentStep: 0,
  currentPage: 'dashboard',
  steps: onboardingSteps,
};

const onboardingSlice = createSlice({
  name: 'onboarding',
  initialState,
  reducers: {
    startOnboarding: (state) => {
      state.isActive = true;
      state.currentStep = 0;
      state.isCompleted = false;
    },
    nextStep: (state) => {
      if (state.currentStep < state.steps.length - 1) {
        state.currentStep += 1;
      } else {
        state.isActive = false;
        state.isCompleted = true;
      }
    },
    previousStep: (state) => {
      if (state.currentStep > 0) {
        state.currentStep -= 1;
      }
    },
    goToStep: (state, action: PayloadAction<number>) => {
      if (action.payload >= 0 && action.payload < state.steps.length) {
        state.currentStep = action.payload;
      }
    },
    setCurrentPage: (state, action: PayloadAction<string>) => {
      state.currentPage = action.payload;
    },
    skipOnboarding: (state) => {
      state.isActive = false;
      state.isCompleted = true;
    },
    resetOnboarding: (state) => {
      state.isActive = false;
      state.isCompleted = false;
      state.currentStep = 0;
    },
  },
});

export const {
  startOnboarding,
  nextStep,
  previousStep,
  goToStep,
  setCurrentPage,
  skipOnboarding,
  resetOnboarding,
} = onboardingSlice.actions;

export default onboardingSlice.reducer;