import React, { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '../../utils/hooks';
import { logout } from '../../store/authSlice';
import OnboardingOverlay from '../common/OnboardingOverlay';
import { useOnboarding } from '../../utils/useOnboarding';
import {
  Button,
  Typography,
  Avatar,
  DropdownMenu,
  DropdownMenuItem,
  DropdownMenuSeparator,
  Sidebar,
  SidebarHeader,
  SidebarContent,
  SidebarNav,
  SidebarNavItem,
  MenuIcon,
  DashboardIcon,
  ClockIcon,
  ReportsIcon,
  PeopleIcon,
  LogoutIcon,
  UserIcon,
  SettingsIcon,
} from '../../ui';

interface LayoutProps {
  children: React.ReactNode;
}

const Layout: React.FC<LayoutProps> = ({ children }) => {
  const [mobileOpen, setMobileOpen] = useState(false);
  
  const navigate = useNavigate();
  const location = useLocation();
  const dispatch = useAppDispatch();
  const { user } = useAppSelector((state) => state.auth);
  const { resetOnboarding } = useOnboarding();

  // Determine current page for onboarding
  const getCurrentPage = () => {
    const path = location.pathname;
    if (path === '/dashboard') return 'dashboard';
    if (path === '/time-entries') return 'time-entries';
    if (path === '/reports') return 'reports';
    return 'dashboard';
  };

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  const handleLogout = () => {
    dispatch(logout());
    navigate('/login');
  };

  const menuItems = [
    { text: 'Dashboard', icon: <DashboardIcon />, path: '/dashboard' },
    { text: 'Time Entries', icon: <ClockIcon />, path: '/time-entries' },
    { text: 'Reports', icon: <ReportsIcon />, path: '/reports' },
  ];

  // Add admin-only menu items
  if (user?.roles?.includes('Administrator') || user?.roles?.includes('Manager')) {
    menuItems.push(
      { text: 'Employees', icon: <PeopleIcon />, path: '/employees' }
    );
  }

  const sidebarContent = (
    <div data-onboarding-target="nav-sidebar">
      <SidebarHeader>
        <Typography variant="h5" className="font-bold text-gray-900">
          Oxigin Attendance
        </Typography>
      </SidebarHeader>
      <SidebarContent>
        <SidebarNav>
          {menuItems.map((item) => (
            <SidebarNavItem
              key={item.text}
              isActive={location.pathname === item.path}
              onClick={() => navigate(item.path)}
              icon={item.icon}
            >
              {item.text}
            </SidebarNavItem>
          ))}
        </SidebarNav>
      </SidebarContent>
    </div>
  );

  return (
    <div className="flex h-screen bg-gray-50">
      {/* Mobile sidebar */}
      <Sidebar variant="mobile" isOpen={mobileOpen} onClose={() => setMobileOpen(false)}>
        {sidebarContent}
      </Sidebar>

      {/* Desktop sidebar */}
      <Sidebar variant="desktop">
        {sidebarContent}
      </Sidebar>

      {/* Main content area */}
      <div className="flex flex-1 flex-col lg:pl-64">
        {/* Header */}
        <header className="sticky top-0 z-30 flex h-16 shrink-0 items-center gap-x-4 border-b border-gray-200 bg-white px-4 shadow-sm sm:gap-x-6 sm:px-6 lg:px-8">
          <Button
            variant="ghost"
            size="icon"
            onClick={handleDrawerToggle}
            className="lg:hidden"
          >
            <MenuIcon className="h-5 w-5" />
          </Button>
          
          <div className="flex flex-1 items-center justify-between">
            <Typography variant="h4" className="text-gray-900">
              {menuItems.find(item => item.path === location.pathname)?.text || 'Dashboard'}
            </Typography>
            
            <div className="flex items-center gap-x-4">
              <Typography variant="body2" className="hidden sm:block text-gray-700">
                {user?.firstName} {user?.lastName}
              </Typography>
              
              <DropdownMenu
                trigger={
                  <Button
                    variant="ghost"
                    size="icon"
                    className="relative h-8 w-8 rounded-full"
                    data-onboarding-target="profile-avatar"
                  >
                    <Avatar size="sm">
                      {user?.firstName?.[0]}{user?.lastName?.[0]}
                    </Avatar>
                  </Button>
                }
              >
                <DropdownMenuItem onClick={() => {}}>
                  <UserIcon className="mr-3 h-4 w-4" />
                  Profile
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => {}}>
                  <SettingsIcon className="mr-3 h-4 w-4" />
                  Settings
                </DropdownMenuItem>
                <DropdownMenuItem onClick={resetOnboarding}>
                  <ClockIcon className="mr-3 h-4 w-4" />
                  Restart Tour
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem onClick={handleLogout} variant="danger">
                  <LogoutIcon className="mr-3 h-4 w-4" />
                  Logout
                </DropdownMenuItem>
              </DropdownMenu>
            </div>
          </div>
        </header>

        {/* Page content */}
        <main className="flex-1 overflow-auto">
          <div className="px-4 py-6 sm:px-6 lg:px-8">
            {children}
          </div>
        </main>
      </div>
      
      {/* Onboarding Overlay */}
      <OnboardingOverlay currentPage={getCurrentPage()} />
    </div>
  );
};

export default Layout;