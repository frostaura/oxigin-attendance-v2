import React, { useState } from 'react';
import {
  Button,
  Card,
  CardHeader,
  CardContent,
  CardTitle,
  Typography,
  Input,
  Alert,
  Badge,
  Avatar,
  Progress,
  DataTable,
  useToast,
  EmptyTimeEntries,
  EmptyReports,
  EmptySearch,
  ThemeToggle,
  SkeletonCard,
  SkeletonList,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  CheckCircleIcon,
  ExclamationTriangleIcon,
  InformationCircleIcon,
  XCircleIcon,
} from '../../ui';

const ComponentShowcase: React.FC = () => {
  const [modalOpen, setModalOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const { addToast } = useToast();

  // Sample data for DataTable
  const sampleData = [
    { id: 1, name: 'John Doe', role: 'Manager', department: 'IT', status: 'Active' },
    { id: 2, name: 'Jane Smith', role: 'Developer', department: 'Engineering', status: 'Active' },
    { id: 3, name: 'Mike Johnson', role: 'Designer', department: 'Design', status: 'Inactive' },
    { id: 4, name: 'Sarah Wilson', role: 'Analyst', department: 'Analytics', status: 'Active' },
  ];

  const columns = [
    { key: 'name', title: 'Name', dataIndex: 'name' as keyof typeof sampleData[0], sortable: true },
    { key: 'role', title: 'Role', dataIndex: 'role' as keyof typeof sampleData[0], sortable: true },
    { key: 'department', title: 'Department', dataIndex: 'department' as keyof typeof sampleData[0], sortable: true },
    {
      key: 'status',
      title: 'Status',
      dataIndex: 'status' as keyof typeof sampleData[0],
      render: (value: string) => (
        <Badge 
          label={value} 
          variant={value === 'Active' ? 'success' : 'secondary'}
        />
      )
    },
  ];

  const showToast = (variant: 'success' | 'error' | 'warning' | 'info') => {
    addToast({
      variant,
      title: `${variant.charAt(0).toUpperCase() + variant.slice(1)} Toast`,
      description: `This is a ${variant} toast notification with UntitledUI styling.`,
    });
  };

  const toggleLoading = () => {
    setLoading(!loading);
  };

  return (
    <div className="space-y-8">
      <div className="flex items-center justify-between">
        <Typography variant="h2" className="text-gray-900 dark:text-white">
          UntitledUI Component Showcase
        </Typography>
        <ThemeToggle />
      </div>

      <Typography variant="body1" className="text-gray-600 dark:text-gray-400">
        Explore the beautiful UntitledUI design system components with awesome enhancements!
      </Typography>

      {/* Buttons Section */}
      <Card>
        <CardHeader>
          <CardTitle>Button Components</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex flex-wrap gap-3">
            <Button variant="primary">Primary</Button>
            <Button variant="secondary">Secondary</Button>
            <Button variant="outline">Outline</Button>
            <Button variant="ghost">Ghost</Button>
            <Button variant="destructive">Destructive</Button>
            <Button variant="link">Link</Button>
          </div>
          <div className="flex flex-wrap gap-3">
            <Button size="sm">Small</Button>
            <Button size="md">Medium</Button>
            <Button size="lg">Large</Button>
            <Button size="xl">Extra Large</Button>
            <Button size="2xl">2X Large</Button>
          </div>
          <div className="flex flex-wrap gap-3">
            <Button startIcon={<CheckCircleIcon />}>With Icon</Button>
            <Button loading>Loading</Button>
            <Button disabled>Disabled</Button>
          </div>
        </CardContent>
      </Card>

      {/* Toast Notifications */}
      <Card>
        <CardHeader>
          <CardTitle>Toast Notifications</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-wrap gap-3">
            <Button variant="outline" onClick={() => showToast('success')}>
              <CheckCircleIcon className="mr-2 h-4 w-4" />
              Success Toast
            </Button>
            <Button variant="outline" onClick={() => showToast('error')}>
              <XCircleIcon className="mr-2 h-4 w-4" />
              Error Toast
            </Button>
            <Button variant="outline" onClick={() => showToast('warning')}>
              <ExclamationTriangleIcon className="mr-2 h-4 w-4" />
              Warning Toast
            </Button>
            <Button variant="outline" onClick={() => showToast('info')}>
              <InformationCircleIcon className="mr-2 h-4 w-4" />
              Info Toast
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Alerts Section */}
      <Card>
        <CardHeader>
          <CardTitle>Alert Components</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <Alert variant="success">This is a success alert with UntitledUI styling!</Alert>
          <Alert variant="error">This is an error alert - something went wrong!</Alert>
          <Alert variant="warning">This is a warning alert - please be careful!</Alert>
          <Alert variant="info">This is an info alert with helpful information.</Alert>
        </CardContent>
      </Card>

      {/* Badges and Avatars */}
      <Card>
        <CardHeader>
          <CardTitle>Badges & Avatars</CardTitle>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="space-y-3">
            <Typography variant="h6">Badges</Typography>
            <div className="flex flex-wrap gap-3">
              <Badge label="Default" variant="default" />
              <Badge label="Success" variant="success" />
              <Badge label="Warning" variant="warning" />
              <Badge label="Error" variant="error" />
              <Badge label="Secondary" variant="secondary" />
            </div>
            <div className="flex flex-wrap gap-3">
              <Badge label="Small" size="sm" />
              <Badge label="Medium" size="md" />
              <Badge label="Large" size="lg" />
            </div>
          </div>
          <div className="space-y-3">
            <Typography variant="h6">Avatars</Typography>
            <div className="flex items-center gap-4">
              <Avatar size="sm">JD</Avatar>
              <Avatar size="md">JS</Avatar>
              <Avatar size="lg">MJ</Avatar>
              <Avatar size="xl">SW</Avatar>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Progress Bars */}
      <Card>
        <CardHeader>
          <CardTitle>Progress Bars</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <Progress value={25} variant="default" />
          <Progress value={50} variant="success" />
          <Progress value={75} variant="warning" />
          <Progress value={90} variant="error" />
        </CardContent>
      </Card>

      {/* Loading States */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center justify-between">
            Skeleton Loading States
            <Button variant="outline" onClick={toggleLoading}>
              {loading ? 'Hide' : 'Show'} Loading
            </Button>
          </CardTitle>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="space-y-6">
              <SkeletonCard />
              <SkeletonList items={3} />
            </div>
          ) : (
            <Typography variant="body2" className="text-gray-500">
              Click "Show Loading" to see skeleton components
            </Typography>
          )}
        </CardContent>
      </Card>

      {/* Data Table */}
      <Card>
        <CardHeader>
          <CardTitle>Data Table</CardTitle>
        </CardHeader>
        <CardContent>
          <DataTable 
            columns={columns} 
            data={sampleData}
            pagination={{
              current: 1,
              pageSize: 10,
              total: 4
            }}
          />
        </CardContent>
      </Card>

      {/* Empty States */}
      <Card>
        <CardHeader>
          <CardTitle>Empty States</CardTitle>
        </CardHeader>
        <CardContent className="space-y-8">
          <div className="border rounded-lg p-6">
            <EmptyTimeEntries onAddEntry={() => addToast({ variant: 'info', title: 'Clock In', description: 'Clock in feature triggered!' })} />
          </div>
          <div className="border rounded-lg p-6">
            <EmptyReports onGenerate={() => addToast({ variant: 'info', title: 'Generate Report', description: 'Report generation triggered!' })} />
          </div>
          <div className="border rounded-lg p-6">
            <EmptySearch searchTerm="nonexistent" onClear={() => addToast({ variant: 'info', title: 'Clear Search', description: 'Search cleared!' })} />
          </div>
        </CardContent>
      </Card>

      {/* Modal */}
      <Card>
        <CardHeader>
          <CardTitle>Modal Component</CardTitle>
        </CardHeader>
        <CardContent>
          <Button onClick={() => setModalOpen(true)}>Open Modal</Button>
        </CardContent>
      </Card>

      {/* Input Components */}
      <Card>
        <CardHeader>
          <CardTitle>Form Components</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <Input label="Regular Input" placeholder="Enter some text..." />
          <Input label="With Error" error="This field is required" placeholder="Invalid input" />
          <Input label="Disabled Input" disabled placeholder="Disabled input" />
        </CardContent>
      </Card>

      {/* Demo Modal */}
      <Modal isOpen={modalOpen} onClose={() => setModalOpen(false)}>
        <ModalHeader>Beautiful Modal</ModalHeader>
        <ModalBody>
          <Typography variant="body1">
            This is a beautiful modal with UntitledUI styling! It includes smooth animations, 
            backdrop blur effects, and proper accessibility features.
          </Typography>
          <div className="mt-4">
            <Input label="Modal Input" placeholder="Type something..." />
          </div>
        </ModalBody>
        <ModalFooter>
          <Button variant="outline" onClick={() => setModalOpen(false)}>
            Cancel
          </Button>
          <Button variant="primary" onClick={() => {
            addToast({ variant: 'success', title: 'Modal Action', description: 'Modal action completed!' });
            setModalOpen(false);
          }}>
            Confirm
          </Button>
        </ModalFooter>
      </Modal>
    </div>
  );
};

export default ComponentShowcase;