import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { 
  VStack, HStack, Field, Input, Button, Textarea, createToaster
} from '@chakra-ui/react';
import { createPlanetarySystem, updatePlanetarySystem } from '../../services/api';
import { emptyPlanetarySystemPayload } from '../../types/dto';

const schema = z.object({
  name: z.string().min(1, 'Nazwa jest wymagana'),
  description: z.string().optional()
});

const defaultToaster = createToaster({
  placement: 'top-end',
  duration: 3000,
});

export default function PlanetarySystemForm({ fractionId, system, onClose, toaster = defaultToaster }) {
  const isEditMode = !!system;
  const queryClient = useQueryClient();

  const { register, handleSubmit, reset, formState: { errors } } = useForm({
    resolver: zodResolver(schema),
    defaultValues: emptyPlanetarySystemPayload()
  });

  // Load existing system data when in edit mode
  useEffect(() => {
    if (system && isEditMode) {
      reset({
        name: system.name,
        description: system.description || ''
      });
    }
  }, [system, isEditMode, reset]);

  const mutation = useMutation({
    mutationFn: (payload) => isEditMode 
      ? updatePlanetarySystem(fractionId, system.id, payload)
      : createPlanetarySystem(fractionId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries(['fleet', fractionId]);
      toaster.success({ title: isEditMode ? 'Układ zaktualizowany' : 'Układ planetarny dodany' });
      onClose();
    },
    onError: (error) => {
      toaster.error({ title: 'Błąd', description: error.message });
    }
  });

  const onSubmit = (data) => {
    mutation.mutate(data);
  };

  return (
    <VStack as="form" align="stretch" gap="4" onSubmit={handleSubmit(onSubmit)}>
        <Field.Root invalid={!!errors.name}>
          <Field.Label>Nazwa układu</Field.Label>
          <Input {...register('name')} placeholder="np. System Alpha Centauri" />
          {errors.name && <Field.ErrorText>{errors.name.message}</Field.ErrorText>}
        </Field.Root>

        <Field.Root>
          <Field.Label>Opis</Field.Label>
          <Textarea 
            {...register('description')} 
            placeholder="Opis układu planetarnego..."
            rows={3}
          />
        </Field.Root>

        <HStack justify="flex-end" gap="2" pt="4">
          <Button variant="outline" onClick={onClose}>Anuluj</Button>
          <Button type="submit" colorPalette="blue" loading={mutation.isPending}>
            {isEditMode ? 'Zapisz zmiany' : 'Dodaj układ'}
          </Button>
        </HStack>
      </VStack>
  );
}

