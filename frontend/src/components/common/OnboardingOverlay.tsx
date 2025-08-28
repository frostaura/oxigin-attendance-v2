import React, { useEffect, useState } from 'react';
import { useAppSelector, useAppDispatch } from '../../utils/hooks';
import {
  nextStep,
  previousStep,
  skipOnboarding,
  setCurrentPage,
} from '../../store/onboardingSlice';
import {
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Typography,
  Button,
  Progress,
  CloseIcon,
  ArrowLeftIcon,
  ArrowRightIcon,
  SkipIcon,
} from '../../ui';

interface OnboardingOverlayProps {
  currentPage: string;
}

const OnboardingOverlay: React.FC<OnboardingOverlayProps> = ({ currentPage }) => {
  const dispatch = useAppDispatch();
  const { isActive, currentStep, steps } = useAppSelector((state) => state.onboarding);
  const [targetElement, setTargetElement] = useState<HTMLElement | null>(null);
  const [highlightStyle, setHighlightStyle] = useState<any>({});

  const currentStepData = steps[currentStep];
  const progress = ((currentStep + 1) / steps.length) * 100;

  useEffect(() => {
    dispatch(setCurrentPage(currentPage));
  }, [currentPage, dispatch]);

  useEffect(() => {
    if (isActive && currentStepData?.target) {
      const element = document.querySelector(`[data-onboarding-target="${currentStepData.target}"]`) as HTMLElement;
      if (element) {
        setTargetElement(element);
        const rect = element.getBoundingClientRect();
        setHighlightStyle({
          position: 'fixed',
          top: rect.top - 8,
          left: rect.left - 8,
          width: rect.width + 16,
          height: rect.height + 16,
          borderRadius: '8px',
          border: '3px solid #2E90FA', // UntitledUI primary color
          background: 'transparent',
          boxShadow: '0 0 0 2000px rgba(0, 0, 0, 0.5)',
          zIndex: 1299,
          pointerEvents: 'none',
          transition: 'all 0.3s ease-in-out',
        });
        
        // Scroll element into view
        element.scrollIntoView({ behavior: 'smooth', block: 'center' });
      } else {
        setTargetElement(null);
        setHighlightStyle({});
      }
    } else {
      setTargetElement(null);
      setHighlightStyle({});
    }
  }, [currentStep, currentStepData, isActive]);

  const handleNext = () => {
    dispatch(nextStep());
  };

  const handlePrevious = () => {
    dispatch(previousStep());
  };

  const handleSkip = () => {
    dispatch(skipOnboarding());
  };

  const isFirstStep = currentStep === 0;
  const isLastStep = currentStep === steps.length - 1;

  if (!isActive || currentStepData?.page !== currentPage) {
    return null;
  }

  return (
    <>
      {/* Highlight overlay */}
      {targetElement && (
        <div style={highlightStyle} />
      )}
      
      {/* Main modal */}
      <Modal 
        isOpen={isActive} 
        onClose={handleSkip}
        size="md"
        className="max-w-lg"
      >
        <div className="relative">
          {/* Progress Bar */}
          <div className="mb-6">
            <Typography variant="caption" color="textSecondary" className="mb-2 block">
              Step {currentStep + 1} of {steps.length}
            </Typography>
            <Progress value={progress} className="h-2" />
          </div>

          {/* Close Button */}
          <Button
            onClick={handleSkip}
            variant="ghost"
            size="icon"
            className="absolute -top-2 -right-2 text-gray-500 hover:text-gray-700"
          >
            <CloseIcon className="h-4 w-4" />
          </Button>

          <ModalHeader className="pr-8">
            {currentStepData.title}
          </ModalHeader>
          
          <ModalBody>
            <Typography variant="body1" color="textSecondary" className="leading-relaxed">
              {currentStepData.content}
            </Typography>
          </ModalBody>

          <ModalFooter className="flex justify-between items-center">
            <Button
              onClick={handleSkip}
              variant="ghost"
              startIcon={<SkipIcon className="h-4 w-4" />}
              className="text-gray-600"
            >
              Skip Tour
            </Button>
            
            <div className="flex gap-2">
              {!isFirstStep && (
                <Button
                  onClick={handlePrevious}
                  variant="outline"
                  startIcon={<ArrowLeftIcon className="h-4 w-4" />}
                >
                  Previous
                </Button>
              )}
              
              <Button
                onClick={handleNext}
                variant="primary"
                endIcon={!isLastStep ? <ArrowRightIcon className="h-4 w-4" /> : undefined}
                className="min-w-[100px]"
              >
                {isLastStep ? 'Finish' : 'Next'}
              </Button>
            </div>
          </ModalFooter>
        </div>
      </Modal>
    </>
  );
};

export default OnboardingOverlay;