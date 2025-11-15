import { useParams, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { useEffect } from 'react';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { createFraction, updateFraction, getFraction } from '../../services/api';
import { useMutation, useQueryClient, useQuery } from '@tanstack/react-query';
import {
  Box, Heading, VStack, HStack, Input, Button, createToaster, Toaster, Spinner
} from '@chakra-ui/react';
import { Field } from '@chakra-ui/react';

const schema = z.object({
  fractionName: z.string().min(1, 'Fraction name is required'),
  playerName: z.string().min(1, 'Player name is required'),
  fractionColor: z.string().regex(/^#[0-9A-Fa-f]{6}$/, 'Must be a valid hex color')
});

const toaster = createToaster({
  placement: 'top-end',
  duration: 3000,
});

export default function FractionForm() {
  const { battleId, fractionId } = useParams();
  const isEditMode = !!fractionId;
  const { register, handleSubmit, formState: { errors }, reset, watch, setValue } = useForm({
    resolver: zodResolver(schema),
    defaultValues: { 
      fractionName: '', 
      playerName: '',
      fractionColor: '#FF0000'
    }
  });
  const qc = useQueryClient();
  const nav = useNavigate();

  const colorValue = watch('fractionColor');

  // Pobierz dane frakcji jeśli tryb edycji
  const { data: fractionData, isLoading } = useQuery({
    queryKey: ['fraction', battleId, fractionId],
    queryFn: () => getFraction(battleId, fractionId),
    enabled: isEditMode
  });

  // Wypełnij formularz danymi frakcji przy edycji
  useEffect(() => {
    if (fractionData && isEditMode) {
      reset({
        fractionName: fractionData.fractionName,
        playerName: fractionData.playerName,
        fractionColor: fractionData.fractionColor
      });
    }
  }, [fractionData, isEditMode, reset]);

  const mutation = useMutation({
    mutationFn: (payload) => isEditMode 
      ? updateFraction(battleId, fractionId, payload)
      : createFraction(battleId, payload),
    onSuccess: () => {
      qc.invalidateQueries(['battle', battleId]);
      toaster.create({ 
        title: isEditMode ? 'Fraction updated' : 'Fraction created', 
        type: 'success' 
      });
      nav(`/pustka-admin-panel/${battleId}`);
    }
  });

  if (isEditMode && isLoading) {
    return <Spinner />;
  }

  return (
    <>
      <Toaster toaster={toaster} />
      <Box>
        <Heading size="md" mb="4">{isEditMode ? 'Edit Fraction' : 'Add Fraction'}</Heading>
        <VStack as="form" align="stretch" spacing="4" onSubmit={handleSubmit((v) => mutation.mutate(v))}>
          <Field.Root invalid={!!errors.fractionName}>
            <Field.Label>Fraction Name</Field.Label>
            <Input {...register('fractionName')} placeholder="Enter fraction name" />
            {errors.fractionName && (
              <Field.ErrorText>{errors.fractionName.message}</Field.ErrorText>
            )}
          </Field.Root>

          <Field.Root invalid={!!errors.playerName}>
            <Field.Label>Player Name</Field.Label>
            <Input {...register('playerName')} placeholder="Enter player name" />
            {errors.playerName && (
              <Field.ErrorText>{errors.playerName.message}</Field.ErrorText>
            )}
          </Field.Root>

          <Field.Root invalid={!!errors.fractionColor}>
            <Field.Label>Fraction Color</Field.Label>
            <HStack>
              <Input 
                type="color" 
                value={colorValue}
                onChange={(e) => setValue('fractionColor', e.target.value)}
                width="100px" 
              />
              <Input {...register('fractionColor')} placeholder="#FF0000" />
            </HStack>
            {errors.fractionColor && (
              <Field.ErrorText>{errors.fractionColor.message}</Field.ErrorText>
            )}
          </Field.Root>

          <Button type="submit" colorScheme="green" isLoading={mutation.isPending}>
            {isEditMode ? 'Update Fraction' : 'Create Fraction'}
          </Button>
        </VStack>
      </Box>
    </>
  );
}
