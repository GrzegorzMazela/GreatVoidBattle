import { createContext, useContext, useState, useCallback } from 'react';
import { AlertModal } from '../components/modals/AlertModal';

/**
 * Notification types
 */
export const NOTIFICATION_TYPES = {
  SUCCESS: 'success',
  ERROR: 'error',
  WARNING: 'warning',
  INFO: 'info',
};

const NotificationContext = createContext(null);

/**
 * Notification Provider - provides unified notification system across the app
 */
export const NotificationProvider = ({ children }) => {
  const [notification, setNotification] = useState({
    isOpen: false,
    title: '',
    message: '',
    variant: 'info',
  });

  const showNotification = useCallback(({ title, message, variant = 'info' }) => {
    setNotification({
      isOpen: true,
      title,
      message,
      variant,
    });
  }, []);

  const hideNotification = useCallback(() => {
    setNotification(prev => ({ ...prev, isOpen: false }));
  }, []);

  // Convenience methods
  const showSuccess = useCallback((message, title = 'Sukces') => {
    showNotification({ title, message, variant: NOTIFICATION_TYPES.SUCCESS });
  }, [showNotification]);

  const showError = useCallback((message, title = 'Błąd') => {
    showNotification({ title, message, variant: NOTIFICATION_TYPES.ERROR });
  }, [showNotification]);

  const showWarning = useCallback((message, title = 'Uwaga') => {
    showNotification({ title, message, variant: NOTIFICATION_TYPES.WARNING });
  }, [showNotification]);

  const showInfo = useCallback((message, title = 'Informacja') => {
    showNotification({ title, message, variant: NOTIFICATION_TYPES.INFO });
  }, [showNotification]);

  const value = {
    showNotification,
    hideNotification,
    showSuccess,
    showError,
    showWarning,
    showInfo,
  };

  return (
    <NotificationContext.Provider value={value}>
      {children}
      <AlertModal
        isOpen={notification.isOpen}
        onClose={hideNotification}
        title={notification.title}
        message={notification.message}
        variant={notification.variant}
      />
    </NotificationContext.Provider>
  );
};

/**
 * Hook to use notifications
 */
export const useNotification = () => {
  const context = useContext(NotificationContext);
  if (!context) {
    throw new Error('useNotification must be used within a NotificationProvider');
  }
  return context;
};

export default NotificationContext;

