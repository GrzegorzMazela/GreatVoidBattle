import { useEffect } from 'react';
import {
  DialogRoot,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogBody,
  DialogFooter,
  DialogCloseTrigger,
  DialogActionTrigger,
} from '@chakra-ui/react';
import { Button } from '@chakra-ui/react';

/**
 * Modal do wyświetlania komunikatów (zamiennik alert)
 */
export const AlertModal = ({ isOpen, onClose, title = 'Informacja', message, variant = 'info' }) => {
  useEffect(() => {
    if (isOpen) {
      const timer = setTimeout(() => {
        onClose();
      }, 5000); // Auto-zamknięcie po 5 sekundach
      return () => clearTimeout(timer);
    }
  }, [isOpen, onClose]);

  const getColorScheme = () => {
    switch (variant) {
      case 'error': return 'red';
      case 'success': return 'green';
      case 'warning': return 'orange';
      default: return 'blue';
    }
  };

  return (
    <DialogRoot open={isOpen} onOpenChange={(e) => !e.open && onClose()} size="md">
      <DialogContent style={{ backgroundColor: '#ffffff', color: '#000000' }}>
        <DialogHeader style={{ borderBottom: '1px solid #e2e8f0' }}>
          <DialogTitle style={{ color: '#000000' }}>{title}</DialogTitle>
        </DialogHeader>
        <DialogCloseTrigger />
        <DialogBody style={{ paddingTop: '1rem', paddingBottom: '1rem' }}>
          <p style={{ color: '#000000' }}>{message}</p>
        </DialogBody>
        <DialogFooter style={{ borderTop: '1px solid #e2e8f0', paddingTop: '1rem' }}>
          <DialogActionTrigger asChild>
            <Button colorScheme={getColorScheme()} onClick={onClose} style={{ color: '#ffffff' }}>
              OK
            </Button>
          </DialogActionTrigger>
        </DialogFooter>
      </DialogContent>
    </DialogRoot>
  );
};
