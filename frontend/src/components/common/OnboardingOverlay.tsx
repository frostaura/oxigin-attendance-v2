import React, { useEffect, useState, useRef } from 'react';
import {
  Modal,
  Box,
  Paper,
  Typography,
  Button,
  IconButton,
  LinearProgress,
  Backdrop,
  Fade,
  useTheme,
} from '@mui/material';
import {
  Close as CloseIcon,
  ArrowBack as PreviousIcon,
  ArrowForward as NextIcon,
  SkipNext as SkipIcon,
} from '@mui/icons-material';
import { useAppSelector, useAppDispatch } from '../../utils/hooks';
import {
  nextStep,
  previousStep,
  skipOnboarding,
  setCurrentPage,
} from '../../store/onboardingSlice';

interface OnboardingOverlayProps {
  currentPage: string;
}

const OnboardingOverlay: React.FC<OnboardingOverlayProps> = ({ currentPage }) => {
  const theme = useTheme();
  const dispatch = useAppDispatch();
  const { isActive, currentStep, steps } = useAppSelector((state) => state.onboarding);
  const [targetElement, setTargetElement] = useState<HTMLElement | null>(null);
  const [highlightStyle, setHighlightStyle] = useState<any>({});
  const overlayRef = useRef<HTMLDivElement>(null);

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
          border: `3px solid ${theme.palette.primary.main}`,
          background: 'transparent',
          boxShadow: `0 0 0 2000px rgba(0, 0, 0, 0.5)`,
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
  }, [currentStep, currentStepData, isActive, theme.palette.primary.main]);

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

  const modalContent = (
    <Box
      sx={{
        position: 'absolute',
        top: '50%',
        left: '50%',
        transform: 'translate(-50%, -50%)',
        width: { xs: '90%', sm: 480 },
        maxHeight: '80vh',
        overflow: 'auto',
      }}
    >
      <Paper
        elevation={8}
        sx={{
          p: 4,
          borderRadius: 2,
          position: 'relative',
        }}
      >
        {/* Progress Bar */}
        <Box sx={{ mb: 3 }}>
          <Typography variant="caption" color="text.secondary" sx={{ mb: 1, display: 'block' }}>
            Step {currentStep + 1} of {steps.length}
          </Typography>
          <LinearProgress 
            variant="determinate" 
            value={progress} 
            sx={{ 
              height: 6, 
              borderRadius: 3,
              backgroundColor: theme.palette.grey[200],
              '& .MuiLinearProgress-bar': {
                borderRadius: 3,
              }
            }} 
          />
        </Box>

        {/* Close Button */}
        <IconButton
          onClick={handleSkip}
          sx={{
            position: 'absolute',
            right: 8,
            top: 8,
            color: 'grey.500',
          }}
        >
          <CloseIcon />
        </IconButton>

        {/* Content */}
        <Typography variant="h5" component="h2" sx={{ mb: 2, pr: 4 }}>
          {currentStepData.title}
        </Typography>
        
        <Typography variant="body1" sx={{ mb: 4, color: 'text.secondary', lineHeight: 1.6 }}>
          {currentStepData.content}
        </Typography>

        {/* Action Buttons */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Button
            onClick={handleSkip}
            color="inherit"
            startIcon={<SkipIcon />}
            sx={{ color: 'text.secondary' }}
          >
            Skip Tour
          </Button>
          
          <Box sx={{ display: 'flex', gap: 1 }}>
            {!isFirstStep && (
              <Button
                onClick={handlePrevious}
                variant="outlined"
                startIcon={<PreviousIcon />}
              >
                Previous
              </Button>
            )}
            
            <Button
              onClick={handleNext}
              variant="contained"
              endIcon={!isLastStep ? <NextIcon /> : undefined}
              sx={{ minWidth: 100 }}
            >
              {isLastStep ? 'Finish' : 'Next'}
            </Button>
          </Box>
        </Box>
      </Paper>
    </Box>
  );

  return (
    <>
      {/* Highlight overlay */}
      {targetElement && (
        <Box sx={highlightStyle} />
      )}
      
      {/* Main modal */}
      <Modal
        open={isActive}
        onClose={handleSkip}
        BackdropComponent={Backdrop}
        BackdropProps={{
          sx: {
            backgroundColor: 'rgba(0, 0, 0, 0.7)',
            backdropFilter: 'blur(2px)',
          },
        }}
      >
        <Fade in={isActive}>
          <div ref={overlayRef}>
            {modalContent}
          </div>
        </Fade>
      </Modal>
    </>
  );
};

export default OnboardingOverlay;