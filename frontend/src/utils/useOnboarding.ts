import { useEffect } from 'react';
import { useAppDispatch, useAppSelector } from './hooks';
import { startOnboarding, skipOnboarding } from '../store/onboardingSlice';

const ONBOARDING_COMPLETED_KEY = 'oxigin-onboarding-completed';

export const useOnboarding = () => {
  const dispatch = useAppDispatch();
  const { isCompleted, isActive } = useAppSelector((state) => state.onboarding);
  const { isAuthenticated, user } = useAppSelector((state) => state.auth);

  useEffect(() => {
    if (isAuthenticated && user) {
      const hasCompletedOnboarding = localStorage.getItem(ONBOARDING_COMPLETED_KEY) === 'true';
      
      if (!hasCompletedOnboarding && !isActive && !isCompleted) {
        // Start onboarding for new users after a short delay
        const timer = setTimeout(() => {
          dispatch(startOnboarding());
        }, 1000);
        
        return () => clearTimeout(timer);
      }
    }
  }, [isAuthenticated, user, isActive, isCompleted, dispatch]);

  useEffect(() => {
    if (isCompleted) {
      localStorage.setItem(ONBOARDING_COMPLETED_KEY, 'true');
    }
  }, [isCompleted]);

  const resetOnboarding = () => {
    localStorage.removeItem(ONBOARDING_COMPLETED_KEY);
    dispatch(startOnboarding());
  };

  const skipOnboardingPermanently = () => {
    dispatch(skipOnboarding());
    localStorage.setItem(ONBOARDING_COMPLETED_KEY, 'true');
  };

  return {
    resetOnboarding,
    skipOnboardingPermanently,
  };
};