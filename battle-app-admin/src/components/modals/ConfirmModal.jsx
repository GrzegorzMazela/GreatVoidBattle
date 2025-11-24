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
import { Button, HStack } from '@chakra-ui/react';

/**
 * Modal do potwierdzania akcji (zamiennik confirm)
 */
export const ConfirmModal = ({ 
  isOpen, 
  onClose, 
  onConfirm, 
  title = 'Potwierdzenie', 
  message,
  confirmText = 'OK',
  cancelText = 'Cancel',
  colorScheme = 'blue'
}) => {
  const handleConfirm = () => {
    onConfirm();
    onClose();
  };

  return (
    <DialogRoot 
      open={isOpen} 
      onOpenChange={(e) => !e.open && onClose()} 
      size="md"
      blockScrollOnMount={false}
      preserveScrollBarGap
    >
      <DialogContent style={{ backgroundColor: '#ffffff', color: '#000000' }}>
        <DialogHeader style={{ borderBottom: '1px solid #e2e8f0' }}>
          <DialogTitle style={{ color: '#000000' }}>{title}</DialogTitle>
        </DialogHeader>
        <DialogCloseTrigger />
        <DialogBody style={{ paddingTop: '1rem', paddingBottom: '1rem' }}>
          <p style={{ color: '#000000' }}>{message}</p>
        </DialogBody>
        <DialogFooter style={{ borderTop: '1px solid #e2e8f0', paddingTop: '1rem' }}>
          <HStack spacing={3}>
            <DialogActionTrigger asChild>
              <Button variant="outline" onClick={onClose} style={{ color: '#000000', borderColor: '#e2e8f0' }}>
                {cancelText}
              </Button>
            </DialogActionTrigger>
            <Button colorScheme={colorScheme} onClick={handleConfirm} style={{ color: '#ffffff' }}>
              {confirmText}
            </Button>
          </HStack>
        </DialogFooter>
      </DialogContent>
    </DialogRoot>
  );
};
