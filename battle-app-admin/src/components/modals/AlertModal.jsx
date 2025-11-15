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
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
        </DialogHeader>
        <DialogCloseTrigger />
        <DialogBody>
          <p>{message}</p>
        </DialogBody>
        <DialogFooter>
          <DialogActionTrigger asChild>
            <Button colorScheme={getColorScheme()} onClick={onClose}>
              OK
            </Button>
          </DialogActionTrigger>
        </DialogFooter>
      </DialogContent>
    </DialogRoot>
  );
};
