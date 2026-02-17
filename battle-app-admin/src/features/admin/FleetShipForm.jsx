import { useEffect } from 'react';
import { useForm, useWatch, useFieldArray } from 'react-hook-form';
import { z } from 'zod';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { 
  VStack, HStack, Field, Input, Button, Box, Text,
  NativeSelectRoot, NativeSelectField
} from '@chakra-ui/react';
import { createFleetShip, updateFleetShip } from '../../services/api';
import { ShipTypes, ShipCategories, ShipDefaultStats, WeaponDefaults, emptyFleetShipPayload, getShipTypeNamePl } from '../../types/dto';
import { useNotification } from '../../contexts/NotificationContext';

const WeaponTypes = ['Missile', 'Laser', 'PointDefense'];

// Schema będzie dynamicznie tworzona w zależności od requireSystem
const createSchema = (requireSystem) => z.object({
  name: z.string().min(1, 'Nazwa jest wymagana'),
  type: z.enum(ShipTypes),
  category: z.enum(ShipCategories),
  speed: z.coerce.number().min(0),
  hitPoints: z.coerce.number().min(1),
  shields: z.coerce.number().min(0),
  armor: z.coerce.number().min(0),
  laserMaxRange: z.coerce.number().min(0).optional(),
  laserDamage: z.coerce.number().min(0).optional(),
  missileMaxRange: z.coerce.number().min(0).optional(),
  missileEffectiveRange: z.coerce.number().min(0).optional(),
  missileDamage: z.coerce.number().min(0).optional(),
  missileSpeed: z.coerce.number().min(0).optional(),
  modules: z.array(z.object({
    weaponTypes: z.array(z.string()).length(3)
  })),
  stationedInSystemId: requireSystem 
    ? z.string().min(1, 'Wybierz układ planetarny')
    : z.string().nullable().optional()
});

export default function FleetShipForm({ fractionId, ship, planetarySystems = [], onClose, requireSystem = false }) {
  const { showSuccess, showError } = useNotification();
  const isEditMode = !!ship;
  const queryClient = useQueryClient();

  const { register, handleSubmit, control, reset, setValue, formState: { errors } } = useForm({
    resolver: zodResolver(createSchema(requireSystem && !isEditMode)),
    defaultValues: emptyFleetShipPayload()
  });
  
  const { fields, replace } = useFieldArray({
    control,
    name: 'modules'
  });

  const shipType = useWatch({ control, name: 'type' });

  // Dla nowego statku we flocie: ustaw domyślne wartości broni w UI przy pierwszym montażu
  useEffect(() => {
    if (!isEditMode) {
      setValue('laserMaxRange', WeaponDefaults.laserMaxRange);
      setValue('laserDamage', WeaponDefaults.laserDamage);
      setValue('missileMaxRange', WeaponDefaults.missileMaxRange);
      setValue('missileEffectiveRange', WeaponDefaults.missileEffectiveRange);
      setValue('missileDamage', WeaponDefaults.missileDamage);
      setValue('missileSpeed', WeaponDefaults.missileSpeed);
    }
  }, [isEditMode, setValue]);

  // Load existing ship data when in edit mode
  useEffect(() => {
    if (ship && isEditMode) {
      reset({
        name: ship.name,
        type: ship.type,
        category: ship.category || 'Combat',
        speed: ship.speed,
        hitPoints: ship.hitPoints,
        shields: ship.shields,
        armor: ship.armor,
        laserMaxRange: ship.laserMaxRange,
        laserDamage: ship.laserDamage,
        missileMaxRange: ship.missileMaxRange,
        missileEffectiveRange: ship.missileEffectiveRange,
        missileDamage: ship.missileDamage,
        missileSpeed: ship.missileSpeed,
        modules: ship.modules?.map(m => ({
          weaponTypes: m.weaponTypes || ['Missile', 'Laser', 'PointDefense']
        })) || [],
        stationedInSystemId: ship.stationedInSystemId || null
      });
    }
  }, [ship, isEditMode, reset]);

  // Update default stats when ship type changes (only for new ships)
  useEffect(() => {
    if (!isEditMode && shipType) {
      const defaults = ShipDefaultStats[shipType];
      if (defaults) {
        setValue('speed', defaults.speed);
        setValue('hitPoints', defaults.hitPoints);
        setValue('shields', defaults.shields);
        setValue('armor', defaults.armor);
        setValue('laserMaxRange', defaults.laserMaxRange);
        setValue('laserDamage', defaults.laserDamage);
        setValue('missileMaxRange', defaults.missileMaxRange);
        setValue('missileEffectiveRange', defaults.missileEffectiveRange);
        setValue('missileDamage', defaults.missileDamage);
        setValue('missileSpeed', defaults.missileSpeed);
        const newModules = Array.from({ length: defaults.modules }, () => ({
          weaponTypes: ['Missile', 'Laser', 'PointDefense']
        }));
        replace(newModules);
      }
    }
  }, [shipType, isEditMode, setValue, replace]);

  const mutation = useMutation({
    mutationFn: (payload) => isEditMode 
      ? updateFleetShip(fractionId, ship.id, payload)
      : createFleetShip(fractionId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries(['fleet', fractionId]);
      showSuccess(isEditMode ? 'Statek zaktualizowany' : 'Statek dodany do floty');
      onClose();
    },
    onError: (error) => {
      showError(`Błąd: ${error.message}`);
    }
  });

  const onSubmit = (data) => {
    mutation.mutate(data);
  };

  return (
    <VStack as="form" align="stretch" gap="4" onSubmit={handleSubmit(onSubmit)}>
        <Field.Root invalid={!!errors.name}>
          <Field.Label>Nazwa statku</Field.Label>
          <Input {...register('name')} placeholder="np. ISS Tytan" />
          {errors.name && <Field.ErrorText>{errors.name.message}</Field.ErrorText>}
        </Field.Root>

        <HStack gap="4">
          <Field.Root flex="1">
            <Field.Label>Typ</Field.Label>
            <NativeSelectRoot>
              <NativeSelectField {...register('type')}>
                {ShipTypes.map(t => <option key={t} value={t}>{getShipTypeNamePl(t)}</option>)}
              </NativeSelectField>
            </NativeSelectRoot>
          </Field.Root>

          <Field.Root flex="1">
            <Field.Label>Kategoria</Field.Label>
            <NativeSelectRoot>
              <NativeSelectField {...register('category')}>
                {ShipCategories.map(c => (
                  <option key={c} value={c}>
                    {c === 'Combat' ? 'Bojowy' : c === 'Research' ? 'Badawczy' : 'Stacja orbitalna'}
                  </option>
                ))}
              </NativeSelectField>
            </NativeSelectRoot>
          </Field.Root>
        </HStack>

        <Box p="4" bg="gray.50" rounded="md">
          <Text fontWeight="bold" mb="3">Parametry statku</Text>
          <HStack gap="4" flexWrap="wrap">
            <Field.Root flex="1" minW="100px">
              <Field.Label>Prędkość</Field.Label>
              <Input type="number" {...register('speed')} />
            </Field.Root>
            <Field.Root flex="1" minW="100px">
              <Field.Label>HP</Field.Label>
              <Input type="number" {...register('hitPoints')} />
            </Field.Root>
            <Field.Root flex="1" minW="100px">
              <Field.Label>Tarcze</Field.Label>
              <Input type="number" {...register('shields')} />
            </Field.Root>
            <Field.Root flex="1" minW="100px">
              <Field.Label>Pancerz</Field.Label>
              <Input type="number" {...register('armor')} />
            </Field.Root>
          </HStack>
        </Box>

        <Box p="4" bg="blue.50" rounded="md">
          <Text fontWeight="bold" mb="3">Broń – zasięg i obrażenia (domyślnie: laser 15/30, rakiety 55/35/20/15)</Text>
          <HStack gap="4" flexWrap="wrap">
            <Field.Root flex="1" minW="90px"><Field.Label>Zasięg lasera</Field.Label><Input type="number" {...register('laserMaxRange')} placeholder={String(WeaponDefaults.laserMaxRange)} /></Field.Root>
            <Field.Root flex="1" minW="90px"><Field.Label>Obr. lasera</Field.Label><Input type="number" {...register('laserDamage')} placeholder={String(WeaponDefaults.laserDamage)} /></Field.Root>
            <Field.Root flex="1" minW="90px"><Field.Label>Zasięg rakiet</Field.Label><Input type="number" {...register('missileMaxRange')} placeholder={String(WeaponDefaults.missileMaxRange)} /></Field.Root>
            <Field.Root flex="1" minW="90px"><Field.Label>Zasięg efektyw. rakiet</Field.Label><Input type="number" {...register('missileEffectiveRange')} placeholder={String(WeaponDefaults.missileEffectiveRange)} /></Field.Root>
            <Field.Root flex="1" minW="90px"><Field.Label>Obr. rakiety</Field.Label><Input type="number" {...register('missileDamage')} placeholder={String(WeaponDefaults.missileDamage)} /></Field.Root>
            <Field.Root flex="1" minW="90px"><Field.Label>Prędk. rakiety</Field.Label><Input type="number" {...register('missileSpeed')} placeholder={String(WeaponDefaults.missileSpeed)} /></Field.Root>
          </HStack>
        </Box>

        {planetarySystems.length > 0 && (
          <Field.Root invalid={!!errors.stationedInSystemId}>
            <Field.Label>
              Stacjonowanie {requireSystem && !isEditMode && <Text as="span" color="red.500">*</Text>}
            </Field.Label>
            <NativeSelectRoot>
              <NativeSelectField {...register('stationedInSystemId')}>
                {(!requireSystem || isEditMode) && <option value="">-- Nieprzypisany --</option>}
                {requireSystem && !isEditMode && <option value="">-- Wybierz układ --</option>}
                {planetarySystems.map(s => (
                  <option key={s.id} value={s.id}>{s.name}</option>
                ))}
              </NativeSelectField>
            </NativeSelectRoot>
            {errors.stationedInSystemId && (
              <Field.ErrorText>{errors.stationedInSystemId.message}</Field.ErrorText>
            )}
          </Field.Root>
        )}

        {fields.length > 0 && (
          <Box p="4" borderWidth="1px" borderRadius="md">
            <Text fontWeight="bold" mb="3">Moduły ({fields.length})</Text>
            <VStack align="stretch" gap="4">
              {fields.map((field, moduleIndex) => (
                <Box key={field.id} p="3" borderWidth="1px" borderRadius="md" bg="gray.50">
                  <Text fontWeight="medium" mb="2">Moduł {moduleIndex + 1}</Text>
                  <HStack gap="2">
                    {[0, 1, 2].map((slotIndex) => (
                      <Field.Root key={slotIndex} flex="1">
                        <Field.Label fontSize="xs">Slot {slotIndex + 1}</Field.Label>
                        <NativeSelectRoot size="sm">
                          <NativeSelectField {...register(`modules.${moduleIndex}.weaponTypes.${slotIndex}`)}>
                            {WeaponTypes.map(wt => <option key={wt} value={wt}>{wt}</option>)}
                          </NativeSelectField>
                        </NativeSelectRoot>
                      </Field.Root>
                    ))}
                  </HStack>
                </Box>
              ))}
            </VStack>
          </Box>
        )}

        <HStack justify="flex-end" gap="2" pt="4">
          <Button variant="outline" onClick={onClose}>Anuluj</Button>
          <Button type="submit" colorPalette="green" loading={mutation.isPending}>
            {isEditMode ? 'Zapisz zmiany' : 'Dodaj statek'}
          </Button>
        </HStack>
      </VStack>
  );
}

